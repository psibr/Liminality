using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PSIBR.Liminality
{
    public class GraphBuilder<TStateMachine>
        where TStateMachine : StateMachine<TStateMachine>
    {
        public GraphBuilder(TStateMachine stateMachine!!)
        {
            _stateMachine = stateMachine;
            _graph = new Graph<TStateMachine>(_stateMachine);
        }

        private readonly TStateMachine _stateMachine;
        private readonly Graph<TStateMachine> _graph;
        private readonly Dictionary<string, GraphNode> _nodeTypeCache = new Dictionary<string, GraphNode>();

        public Graph<TStateMachine> Build()
        {
            var rootNode = new GraphNode(_stateMachine.Definition.StateMap.InitialState);
            foreach (var stateMap in _stateMachine.Definition.StateMap)
            {
                var input = stateMap.Key;
                var transition = stateMap.Value;

                //Enables us to continue node transit mapping
                //in many to many situations
                var subNode = GetOrCreateSubNode(rootNode, input.CurrentStateType, input.SignalType);
                var goesToNode = CreateNode(transition.NewStateType);
                subNode.Add(goesToNode);
            }

            _graph.Add(rootNode);

            return _graph;
        }

        private GraphNode GetOrCreateSubNode(GraphNode rootNode, Type type, Type? signalType)
        {
            var key = MakeCacheKey(type, signalType);
            //If we know about this, return so we can continue
            //mapping the transit
            if (_nodeTypeCache.ContainsKey(type.Name))
                return _nodeTypeCache[type.Name];
            //If we don't know about this but the root
            //and the type are the same, this is a mapping
            //of initial state -> something else
            else if (rootNode.Name == type.Name && rootNode.Condition == signalType?.Name)
                return rootNode;

            var node = CreateNode(type, signalType);
            rootNode.Add(node);
            return node;
        }

        private GraphNode CreateNode(Type type)
        {
            var node = new GraphNode(type, null);
            CacheNodeType(node);
            return node;
        }

        private GraphNode CreateNode(Type type!!, Type? signalType)
        {
            var node = new GraphNode(type, signalType);
            CacheNodeType(node);
            return node;
        }

        private void CacheNodeType(GraphNode graphNode!!)
        {
            var key = MakeCacheKey(graphNode);
            if (!_nodeTypeCache.ContainsKey(key))
                _nodeTypeCache[key] = graphNode;
        }

        private string MakeCacheKey(GraphNode graphNode!!)
        {
            var key = $"{graphNode.Name} {(graphNode.Condition is not null ? $":{graphNode.Condition}" : string.Empty)}";
            return key;
        }

        private string MakeCacheKey(Type type, Type? signalType)
        {
            var key = $"{type.Name} {(signalType is not null ? $":{signalType.Name}" : string.Empty)}";
            return key;
        }
    }

    [DebuggerDisplay("Name = {Name}, Goes to: {Count} other node(s)")]
    public class Graph<TStateMachine> : GraphNode
        where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly TStateMachine _stateMachine;

        public Graph(TStateMachine stateMachine!!) 
            : base(stateMachine.GetType().Name)
        {
            _stateMachine = stateMachine;
        }
    }

    [DebuggerDisplay("Name = {Name}, Goes to: {Count} other node(s) when signaled with {Condition}")]
    public class GraphNode : List<GraphNode>
    {
        public GraphNode(Type type!!)
            : this(type.Name, null)
        {
        }

        public GraphNode(Type type!!, Type? signalType)
            : this(type.Name, signalType?.Name)
        {
        }

        public GraphNode(string name!!)
            : this(name, null)
        {
        }

        public GraphNode(string name!!, string? condition)
        {
            Name = name;
            Condition = condition;
        }

        public string Name { get; set; }
        public string? Condition { get; set; }
    }
}
