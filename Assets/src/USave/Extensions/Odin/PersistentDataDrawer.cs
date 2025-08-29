#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using USave.Data;

namespace USave.Extensions.Odin
{
    public class PersistentDataDrawer : OdinAttributeProcessor
    {
        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            Type type = parentProperty.ValueEntry?.TypeOfValue;
            if (type == null) return false;

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PersistentData<>);
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            if (member.Name == "m_value")
                attributes.Add(new HideLabelAttribute());
        }
    }
}
#endif