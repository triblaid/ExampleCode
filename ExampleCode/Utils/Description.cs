using System;
using System.ComponentModel;
using System.Reflection;

namespace ExampleCode.Utils
{
	public static class Description
	{
		public static string GetDescription(Enum enumElement)
		{
			Type type = enumElement.GetType();

			MemberInfo[] memInfo = type.GetMember(enumElement.ToString());
			if (memInfo != null && memInfo.Length > 0)
			{
				object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
				if (attrs != null && attrs.Length > 0)
					return ((DescriptionAttribute)attrs[0]).Description;
			}

			return enumElement.ToString();
		}
	}
}
