using System;
using System.Reflection;
using Pdoxcl2Sharp.Converters;

namespace Pdoxcl2Sharp.Parsers
{
    internal class DecodeDict : DecodeProperty
    {
        private readonly MethodInfo _addFn;
        private readonly TextConvert _children;
        
        public override Type ChildType { get; }
        public override PropertyType ChildFormat { get; }

        public Type KeyType { get; }
        public PropertyType KeyFormat { get; }
        
        public DecodeDict(
            PropertyInfo property,
            Type keyType,
            PropertyType keyFormat,
            TextConvert children,
            Type childType,
            PropertyType childFormat) : base(property, PropertyType.Array)
        {
            _addFn = property.PropertyType.GetMethod("Add");
            _children = children;
            KeyType = keyType;
            KeyFormat = keyFormat;
            ChildType = childType;
            ChildFormat = childFormat;
        }

        public override void Decode(ref ParadoxTextReader reader, object obj, ParadoxSerializerOptions options)
        {
            var child = _children.BaseRead(ref reader, typeof(int), options);
            _addFn.Invoke(obj, new[] { "abc", child });
        }

        public override void AddChild(object obj, object nestedObj)
        {
            _addFn.Invoke(obj, new[] {"abc1", nestedObj});
            //base.AddChild(obj, nestedObj);
        }
    }
}
