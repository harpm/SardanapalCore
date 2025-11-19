
namespace Sardanapal.Share.Extensions;

public static class TypesExtensions
{
    public static bool ImplementsRawGeneric(Type type, Type generic)
    {
        // check interfaces
        if (type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == generic))
            return true;

        // check base classes too
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
                return true;

            type = type.BaseType;
        }

        return false;
    }

    public static bool IsSubClassOfRawGeneric(this Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}
