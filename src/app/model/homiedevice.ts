import { JsonObject, JsonMember } from '@upe/typedjson';
import * as timespan from "timespan";

@JsonObject()
export class OTA {
  @JsonMember({ name: "enabled" })
  public Enabled: boolean;
}

@JsonObject()
export class MQTT {
  @JsonMember({ name: "host" })
  public Host: string;
  @JsonMember({ name: "port" })
  public Port: number;
  @JsonMember({ name: "base_topic" })
  public BaseTopic: string;
  @JsonMember({ name: "auth" })
  public Auth: boolean;
  @JsonMember({ name: "username" })
  public Username: string;
  @JsonMember({ name: "password" })
  public Password: string;
}

@JsonObject()
export class WiFi {
  @JsonMember({ name: "ssid" })
  public SSID: string;
  @JsonMember({ name: "password" })
  public Password: string;
  @JsonMember({ name: "bssid" })
  public BSSID: string;
  @JsonMember({ name: "chanel" })
  public Channel: number;
  @JsonMember({ name: "ip" })
  public IP: string;
  @JsonMember({ name: "mask" })
  public Mask: string;
  @JsonMember({ name: "gw" })
  public GW: string;
  @JsonMember({ name: "dns1" })
  public DNS1: string;
  @JsonMember({ name: "dns2" })
  public DNS2: string;
}

@JsonObject()
export class DeviceConfig {
  @JsonMember({ name: "name" })
  public Name: string;
  @JsonMember({ name: "device_id" })
  public DeviceID: string;
  @JsonMember({ name: "wifi" })
  public WiFi: WiFi;
  @JsonMember({ name: "mqtt" })
  public MQTT: MQTT;
  @JsonMember({ name: "ota" })
  public OTA: OTA;
  @JsonMember({ name: "settings" })
  public Settings: object;

  getSettingsKeys(): Array<string> {
    if (this.Settings != null)
      return Object.keys(this.Settings);
    else
      return [];
  }
}

@JsonObject()
export class NodeProperty {
  @JsonMember({ name: "mqtt_root_topic_level" })
  public MQTTRootTopicLevel: string;
  @JsonMember({ name: "property_id" })
  public PropertyID: string;
  @JsonMember({ name: "settable" })
  public Settable: boolean;
  @JsonMember({ name: "value" })
  public Value: string;

  update(newObj: NodeProperty): void {
    update(this, newObj);
  }
}

@JsonObject()
export class DeviceNode {
  @JsonMember({ name: "mqtt_root_topic_level" })
  public MQTTRootTopicLevel: string;
  @JsonMember({ name: "node_id" })
  public NodeID: string;
  @JsonMember({ name: "type" })
  public Type: string;
  @JsonMember({ name: "properties", type: Array, elements: NodeProperty })
  public Properties: Array<NodeProperty>;

  update(newObj: DeviceNode): void {
    update(this, newObj);
  }

}

@JsonObject()
export class HomieDevice {
  @JsonMember({ name: "hm_device_id" })
  public HMDeviceID: string;
  @JsonMember({ name: "mqtt_root_topic_level" })
  public MQTTRootTopicLevel: string;
  @JsonMember({ name: "version" })
  public Version: string;
  @JsonMember({ name: "status" })
  public Status: boolean;
  @JsonMember({ name: "name" })
  public Name: string;
  @JsonMember({ name: "local_ip" })
  public LocalIP: string;
  @JsonMember({ name: "mac" })
  public MAC: string;
  @JsonMember({ name: "uptime" })
  public UpTime: string;
  @JsonMember({ name: "signal" })
  public Signal: string;
  @JsonMember({ name: "update_interval" })
  public UpdateInterval: string;
  @JsonMember({ name: "fw_name" })
  public FWName: string;
  @JsonMember({ name: "fw_version" })
  public FWVersion: string;
  @JsonMember({ name: "fw_checksum" })
  public FWCheckSum: string;
  @JsonMember({ name: "implementation" })
  public Implementation: string;
  @JsonMember({ name: "implementation_version" })
  public ImplementationVersion: string;
  @JsonMember({ name: "config" })
  public Config: DeviceConfig;
  @JsonMember({ name: "ota_enabled" })
  public OTAEnabled: boolean;
  @JsonMember({ name: "ota_status" })
  public OTAStatus: string;
  @JsonMember({ name: "nodes", type: Array, elements: DeviceNode })
  public Nodes: Array<DeviceNode>;

  update(newObj: HomieDevice): void {
    update(this, newObj);
  }

  getUpTime(): string {
    let span = timespan.fromSeconds(this.UpTime);
    span.format = (span) => {
      return `Day: ${span.days}, Hour: ${span.hours}, Min: ${span.minutes} Sec: ${span.seconds}`
    }
    return span.toString();
  }
}

@JsonObject()
export class HomieDevicePage {
  @JsonMember({ name: "homie_devices", type: Array, elements: HomieDevice })
  public HomieDevices: Array<HomieDevice>;
}

function update(target: object, source: object) {
  Object.keys(target).forEach(key => {
    if (target[key] != undefined && source[key] != undefined) {
      if (typeof target[key] == typeof source[key]) {
        if (target[key] instanceof Object)
          update(target[key], source[key]);
        else
          target[key] = source[key];
      }
    }
  });
};
