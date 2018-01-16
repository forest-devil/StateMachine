using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine
{
    /// <summary>
    /// 单例访问器，从Abp里借用
    /// </summary>
    public static class Singleton<T> where T : new()
    {
        private static readonly Lazy<T> LazyInstance;

        static Singleton()
        {
            LazyInstance = new Lazy<T>(() => new T(), true);
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static T Instance => LazyInstance.Value;
    }
}