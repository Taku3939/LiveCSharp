using LiveClient;

public static class VLLNetwork
{
    /// <summary>
    /// このClientインスタンスを一番初めに取得するのはメインスレッドでなくてはならない
    /// </summary>
    public static Client Client
    {
        get
        {
            if (client== null) client = new Client();
            return client;
        }
    }

    private static Client client;
}
