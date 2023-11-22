namespace com.chalkline.wopi.Models.Wopi
{
    /// <summary>
    /// Represents the base information determined at the beginning of a WOPI request
    /// </summary>
    public class WopiRequest
    {
        public string Id { get; set; }
        public WopiRequestType RequestType { get; set; }
        public string AccessToken { get; set; }
    }
}
