﻿using IronJS.Runtime.Js;
using IronJS.Runtime.Js.Descriptors;

namespace IronJS.Runtime.Builtins
{
    class Object_prototype_valueOf : NativeFunction
    {
        public Object_prototype_valueOf(Context context)
            : base(context)
        {
            Set("length",
                new UserProperty(this, 0.0D)
            );
        }

        public override object Call(IObj that, object[] args)
        {
            return that;
        }
    }
}
