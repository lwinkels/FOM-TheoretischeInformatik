using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DFACut.DFA;

namespace DFACut
{
    internal class DFA
    {
        #region subclasses
        public class Transition
        {
            public State src { get; private set; }
            public State dst { get; private set; }
            public char symbol { get; private set; }

            public Transition(State src, State dst, char symbol)
            {
                this.src = src;
                this.dst = dst;
                this.symbol = symbol;
            }

            public override bool Equals(object? other)
            {
                if (other is Transition)
                {
                    return src.Equals(((Transition)other).src) && dst.Equals(((Transition)other).dst) && symbol.Equals(((Transition)other).symbol);  
                }
                else
                {
                    return base.Equals(other);
                }
            }

            public override int GetHashCode()
            {
                return src.GetHashCode() + dst.GetHashCode() + symbol.GetHashCode();
            }
        }

        public class State
        {
            public string id { get; private set; }

            public State(string id) 
            {
                this.id = id;
            }

            public override string ToString()
            {
                return id;
            }

            public override bool Equals(object? other)
            {
                if (other is State)
                {
                    return id.Equals(((State)other).id);
                }
                else
                {
                    return base.Equals(other);
                }
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
        }
        #endregion

        #region intersection
        public static DFA Intersect(DFA dfa1, DFA dfa2)
        {
            //#states
            HashSet<State> states = new HashSet<State>();             
            foreach (State s1 in dfa1.states)
            {
                foreach (State s2 in dfa2.states)
                {
                    states.Add(new State(s1.ToString() + "_" + s2.ToString()));
                }
            }

            //#initial
            State initial = new State(dfa1.inital?.ToString() + "_" + dfa2.inital?.ToString());

            //#accepting
            HashSet<State> accepting = new HashSet<State>();
            foreach (State s1 in dfa1.accepting)
            {
                foreach (State s2 in dfa2.accepting)
                {
                    accepting.Add(new State(s1.ToString() + "_" + s2.ToString()));
                }
            }

            //#alphabet
            HashSet<char> alphabet = dfa1.alphabet.Union(dfa2.alphabet).ToHashSet();

            //#transition
            HashSet<Transition> transitions = new HashSet<Transition>();
            foreach (Transition t1 in dfa1.transitions)
            {
                foreach (Transition t2 in dfa2.transitions)
                {
                    if (t1.symbol.Equals(t2.symbol))
                    {
                        Transition t = new Transition(new State(t1.src + "_" + t2.src), new State(t1.dst + "_" + t2.dst), t1.symbol);
                        transitions.Add(t);
                    }
                }
            }

            return new DFA(states, initial, accepting, alphabet, transitions);         
        }
        #endregion

        #region parse
        enum Status
        {
            None,
            States,
            Initial,
            Accepting,
            Alphabet,
            Transitions
        };
        public static DFA parseDFA(string dfaText)
        {
            Status status = Status.None; 
            DFA dfa = new DFA(); 
          
            using (StringReader sr = new StringReader(dfaText))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    
                    if (line.StartsWith("#"))
                    {
                        status = setStatus(line);
                    }
                    else
                    {
                        switch (status)
                        {
                            case Status.States:
                                dfa.states.Add(parseState(line));
                                break;
                            case Status.Initial:
                                dfa.inital = parseState(line);
                                break;
                            case Status.Accepting:
                                dfa.accepting.Add(parseState(line));
                                break;
                            case Status.Alphabet:
                                dfa.alphabet.Add(parseSymbol(line));
                                break;
                            case Status.Transitions:
                                dfa.transitions.UnionWith(parseTransition(line)); 
                                break;
                            default:
                                Console.WriteLine("Syntax Error: " + line);
                                Environment.Exit(0);
                                break;
                        }
                    }
                }
            }

            return dfa;
        }

        private static Status setStatus(string line)
        {
            switch (line.ToLower().Trim())
            {
                case "#states":
                    return Status.States;                    
                case "#initial":
                    return Status.Initial;
                case "#accepting":
                    return Status.Accepting;
                case "#alphabet":
                    return Status.Alphabet;
                case "#transitions":
                    return Status.Transitions;                    
                default:
                    return Status.None;                    
            }
        }
        private static State parseState(string line)
        {
            return new State(line);
        }
        private static char parseSymbol(string line)
        {
            if (line.Length == 1)
            {
                return line[0];
            }
            else
            {
                Console.WriteLine("Syntax Error: " + line);
                Environment.Exit(0);
                return '\0';
            }            
        }

        private static List<Transition> parseTransition(string line)
        {
            List<Transition> result = new List<Transition>();
            // t0:1,2,3,4,5,6,7,8,9>t3
            int srcSep = line.IndexOf(':');
            int dstSep = line.IndexOf('>');
            if ((srcSep != -1) && (dstSep != -1))
            {
                string s = line.Substring(0, srcSep);
                string d = line.Substring(dstSep + 1);

                foreach (string symb in line.Substring(srcSep + 1, dstSep - srcSep - 1).Split(new char[] { ',' }))
                {
                    if (symb.Length == 1)
                    {
                        result.Add(new Transition(new State(s), new State(d), symb[0]));
                    }
                    else
                    {
                        Console.WriteLine("Syntax Error: " + line);
                        Environment.Exit(0);
                        return null;
                    }
                }
            }


            return result;          
        }
        #endregion

        //----------------------------------------------------------

        private HashSet<State> states;       
        private State? inital;
        private HashSet<State> accepting;
        private HashSet<char> alphabet;
        private HashSet<Transition> transitions;

        public DFA() : this (new HashSet<State>(), null, new HashSet<State>(),new HashSet<char>(), new HashSet<Transition>()) { }

        public DFA(HashSet<State> states, State? inital, HashSet<State> accepting, HashSet<char> alphabet, HashSet<Transition> transitions)
        {
            this.states = states;
            this.inital = inital;
            this.accepting = accepting;
            this.alphabet = alphabet;
            this.transitions = transitions;               
        }

        public bool isValid()
        {
            if ((states == null) || (inital == null) || (accepting == null) || (alphabet == null) || (transitions == null))
            { 
                return false;   
            }

            if (!states.Contains(inital)) 
            { 
                return false;
            }

            if (!transitions.All(x => (states.Contains(x.src) && states.Contains(x.dst) && alphabet.Contains(x.symbol))))
            {
                foreach(Transition t in transitions)
                {
                    if (!states.Contains(t.src))
                    {
                        Console.WriteLine(t.src.ToString() + " ist das Problem");
                    }
                }
                
            }

            if (!accepting.All(x => states.Contains(x)))
            {
                return false;
            }

            return true;
        }

        public void Optimize()
        {
            HashSet<State> reachableStates = new HashSet<State>();
            State currState = inital;


            ReachableStates(currState, ref reachableStates);

            reachableStates = RemoveDeadEnds(reachableStates, ref transitions);


            
            HashSet<char> relevantSymbols = transitions.Select(x => x.symbol).ToHashSet();

            this.states = reachableStates;
            //this.transitions = relevantTransitions;
            this.alphabet = relevantSymbols;
        }

        private HashSet<State> RemoveDeadEnds(HashSet<State> reachableState, ref HashSet<Transition> reachableTransitions)
        {
          
            int startSize = reachableState.Count;
            reachableState.RemoveWhere(x => (!accepting.Contains(x) && !transitions.Any(y => y.src.Equals(x))));
            reachableTransitions = reachableTransitions.Where(x => (reachableState.Contains(x.src) && reachableState.Contains(x.dst))).ToHashSet();
            if (startSize > reachableState.Count)
            {
                reachableState = RemoveDeadEnds(reachableState, ref reachableTransitions);
            }
            return reachableState;
        }

        private void ReachableStates(State currState, ref HashSet<State> reachableStates)
        {
            reachableStates.Add(currState);
            HashSet<State> sl = transitions.Where(x => (x.src.Equals(currState))).Select(x => x.dst).ToHashSet();            
            foreach (State s in sl)
            {
                if (!reachableStates.Contains(s))
                {
                    ReachableStates(s, ref reachableStates);
                }
            }
        }


        #region print
        public string printDFA()
        {
            if ( !isValid())
            {
                return "Invalid DFA.";
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(printStates());
            sb.Append(printInitial());
            sb.Append(printAccepting());
            sb.Append(printAlphabet());
            sb.Append(printTransitions());

            return sb.ToString();
        }


        private string printStates()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#states");
            foreach(State state in states) 
            {
                sb.AppendLine(state.ToString());    
            }

            return sb.ToString();
        }

        private string printInitial()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#initial");
            sb.AppendLine(inital?.ToString());

            return sb.ToString();
        }

        private string printAccepting()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#accepting");
            foreach (State state in accepting)
            {
                sb.AppendLine(state.ToString());
            }

            return sb.ToString();
        }
        private string printAlphabet()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#alphabet");
            foreach (char c in alphabet)
            {
                sb.AppendLine(c.ToString());
            }
            return sb.ToString();
        }
        private string printTransitions()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#transitions");

            foreach (IGrouping<State, Transition> srcGroup in transitions.GroupBy(k => k.src))
            {
                foreach (IGrouping<State, Transition> dstGroup in srcGroup.GroupBy(k => k.dst))
                {
                    sb.Append(srcGroup.Key.ToString());
                    sb.Append(":");
                    List<Transition> transitions = dstGroup.ToList();
                    for (int i = 0; i < transitions.Count; i++)
                    {
                        sb.Append(transitions[i].symbol);
                        if (i + 1 < transitions.Count)
                        {
                            sb.Append(',');
                        }
                    }
                    sb.Append('>');
                    sb.Append(dstGroup.Key.ToString());
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
        #endregion

    }
}
