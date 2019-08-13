using FastDeepCloner;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Rest.API.Translator
{
    internal class DynamicTaskCompletionSource
    {
        private static SafeValueType<Type, Type> SourceCacher = new SafeValueType<Type, Type>();
        private static SafeValueType<Type, IFastDeepClonerProperty> TaskProperty = new SafeValueType<Type, IFastDeepClonerProperty>();

        private static SafeValueType<Type, MethodInfo> TrySetResult = new SafeValueType<Type, MethodInfo>();

        private object TaskCompletionSource;

        public IAsyncResult Task { get => TaskProperty.Get(DataType).GetValue(TaskCompletionSource) as IAsyncResult; }

        private Type DataType;
        public DynamicTaskCompletionSource(object items, Type dataType)
        {
            DataType = dataType;
            if (SourceCacher.ContainsKey(dataType))
                TaskCompletionSource = SourceCacher.Get(dataType).CreateInstance();
            else TaskCompletionSource = SourceCacher.GetOrAdd(dataType, typeof(TaskCompletionSource<>).MakeGenericType(dataType)).CreateInstance();


            if (!TrySetResult.ContainsKey(dataType))
                TrySetResult.Add(dataType, TaskCompletionSource.GetType().GetMethod("TrySetResult"));

            TrySetResult.Get(dataType).Invoke(TaskCompletionSource, new object[] { items });

            if (!TaskProperty.ContainsKey(dataType))
                TaskProperty.Add(dataType, DeepCloner.GetProperty(TaskCompletionSource.GetType(), "Task"));
        }
    }
}