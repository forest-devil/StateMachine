using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine
{
    /// <summary>
    /// 工作流单例，由于类型参数包含TStatus，故每个TStatus都有一个单例
    /// 封装了工作流(状态机)的逻辑，内部使用两层嵌套的Dictionary实现
    /// </summary>
    public abstract class Workflow : IWorkflow
    {
        private bool _sealed = false;

        private readonly Dictionary<object, Dictionary<object, object>> _dictionary
             = new Dictionary<object, Dictionary<object, object>>();

        private readonly Lazy<IEnumerable> _validOperations;

        private readonly Lazy<IEnumerable> _validStatuses;

        protected Workflow()
        {
            _validOperations = new Lazy<IEnumerable>(
                () => _dictionary.SelectMany(kvp => kvp.Value).Select(kvp => kvp.Key).Distinct());
            _validStatuses = new Lazy<IEnumerable>(
                () => _dictionary.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key)
                    .Concat(_dictionary.SelectMany(kvp => kvp.Value.Values)).Distinct());
        }

        public bool Sealed
        {
            get => _sealed;
            private set
            {
                if (!_sealed && value == true)
                {
                    var emptyKeys = _dictionary.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
                    foreach (var key in emptyKeys)
                    {
                        _dictionary.Remove(key);
                    }
                    _sealed = value;
                }
            }
        }

        public IEnumerable ValidOperations => _sealed ? _validOperations.Value : new List<object>();
        public IEnumerable ValidStatuses => _sealed ? _validStatuses.Value : new List<object>();

        public IWorkflow AddRule(object status, params (object operation, object result)[] rhs)
        {
            if (Sealed)
                throw new InvalidOperationException();

            foreach (var (operation, result) in rhs)
                AddRule(status, operation, result);

            return this;
        }

        public IWorkflow AddRule(object status, object operation, object result)
        {
            if (Sealed)
                throw new InvalidOperationException();

            var oprations = _dictionary[status];
            if (!oprations.ContainsKey(operation))
                oprations.Add(operation, result);

            return this;
        }

        /// <summary>
        /// 密封工作流状态机
        /// 密封之前可以用AddRule()定义新的状态转换，但Transition()、ValidStatuses、ValidOperations都不可用
        /// 密封之后不能再定义状态转换，上述几个成员变为可用
        /// 密封操作会清理状态机中空的节点
        /// </summary>
        public IWorkflow Seal()
        {
            Sealed = true;
            return this;
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">操作</param>
        /// <returns>this</returns>
        public virtual object Transition(object status, object operation)
        {
            if (!Sealed)
                throw new InvalidOperationException();

            try
            {
                return _dictionary[status][operation];
            }
            catch (NullReferenceException) { }
            catch (KeyNotFoundException) { }
            return status;
        }
    }
}