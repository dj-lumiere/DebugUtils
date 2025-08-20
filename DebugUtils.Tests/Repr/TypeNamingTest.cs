using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Tests;

public class TypeNamingTest
{
    [Fact]
    public void GetReprTypeNameTest()
    {
        var intType = typeof(int);
        Assert.Equal(expected: "int", actual: intType.GetReprTypeName());

        var listType = typeof(List<string>);
        Assert.Equal(expected: "List", actual: listType.GetReprTypeName());

        var intNullableType = typeof(int?);
        Assert.Equal(expected: typeof(int?), actual: typeof(int?));
        Assert.Equal(expected: "int?", actual: intNullableType.GetReprTypeName());

        var taskType = typeof(Task<bool>);
        Assert.Equal(expected: "Task<bool>", actual: taskType.GetReprTypeName());

        var dictType = typeof(Dictionary<string, List<int>>);
        Assert.Equal(expected: "Dictionary", actual: dictType.GetReprTypeName());

        var multiDimArrayType = typeof(int[,]);
        Assert.Equal(expected: "2DArray", actual: multiDimArrayType.GetReprTypeName());

        var jaggedArrayType = typeof(int[][]);
        Assert.Equal(expected: "JaggedArray", actual: jaggedArrayType.GetReprTypeName());

        var nestedType = typeof(OuterClass.NestedClass);
        Assert.Equal(expected: "NestedClass", actual: nestedType.GetReprTypeName());

        var genericMethodType = typeof(TypeNamingTest).GetMethod(name: nameof(GenericMethod))
                                                     ?.GetGenericArguments()[0];
        Assert.Null(@object: genericMethodType?.GetReprTypeName());
    }

    public class OuterClass
    {
        public class NestedClass
        {
        }
    }

    private void GenericMethod<T>()
    {
    }
}