using Newtonsoft.Json;

namespace HomieManagement.Model
{
  // Classes
  public class Result
  {
    [JsonProperty("success")]
    public bool Success { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }

    public Result()
    {

    }
    public Result(bool success)
    {
      Success = success;
    }
    public Result(bool success, string message)
    {
      Success = success;
      Message = message;
    }
  }


}
