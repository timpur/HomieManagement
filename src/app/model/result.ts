import { JsonObject, JsonMember } from '@upe/typedjson';

@JsonObject()
export class Result {
  @JsonMember({ name: "success" })
  public Success: boolean;
  @JsonMember({ name: "message" })
  public Message: string;

  public constructor(success: boolean = false, message: string = null) {
    this.Success = success;
    this.Message = message;
  }
}
