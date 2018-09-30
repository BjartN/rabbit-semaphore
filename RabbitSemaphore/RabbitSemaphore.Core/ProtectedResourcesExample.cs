namespace RabbitSemaphore.Consumer
{
   
    public class ProtectedResourcesExample
    {
        public static string FileShare = "the-file-share";
        public static string Api = "some-api";
        public static string AnotherApi = "some-api-2";

        public static string[] All = {
            FileShare,
            Api,
            AnotherApi
        };

    }
}
