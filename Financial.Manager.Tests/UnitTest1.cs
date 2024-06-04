namespace Financial.Manager.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Guid myuuid = Guid.NewGuid();
        string myuuidAsString = myuuid.ToString();

        Guid sameUuid = new Guid(myuuidAsString);
        Assert.Equal(sameUuid, myuuid);

    }
}