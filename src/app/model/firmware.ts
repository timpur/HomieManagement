import { JsonObject, JsonMember } from '@upe/typedjson';



export enum FirmwareType {
    None,
    Homie_ESP8266
}

@JsonObject()
export class Firmware {
    @JsonMember({ name: "id" })
    public ID: number;
    @JsonMember({ name: "name" })
    public Name: string;
    @JsonMember({ name: "version" })
    public Version: string;
    @JsonMember({ name: "description" })
    public Description: string;
    @JsonMember({ name: "date_created" })
    public DateCreated: Date;
    @JsonMember({ name: "type" })
    public Type: FirmwareType;
    @JsonMember({ name: "disabled" })
    public Disabled: boolean;

    constructor() {

    }

    setDate(date: string): void {
        this.DateCreated = new Date(date);
        this.DateCreated.setMinutes(this.DateCreated.getMinutes() - this.DateCreated.getTimezoneOffset());
    }
}
