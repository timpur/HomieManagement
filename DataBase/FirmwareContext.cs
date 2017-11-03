using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HomieManagement.Model.DataBase
{
  public class HomieManagementContext : DbContext
  {

    //Firmware List (Table)
    public DbSet<Firmware> FirmwarList { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite("Data Source=homiemanagement.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Firmware>().ToTable("Firmware");
    }

    public void Initialize()
    {
      Database.EnsureCreated();
      if (!FirmwarList.Any())
      {
        AddFirmware(new Firmware() { Name = "Test Firmware (Not Valid)", Version = "1.0.0" });
      }
    }


    public List<Firmware> FindFirmwareVersionChain(string firmwareName)
    {
      return FirmwarList.Where(firmware => firmware.Name == firmwareName).OrderBy(firmware => firmware.Version).ToList();
    }

    public Firmware FindFirmware(int id)
    {
      return FirmwarList.Find(id);
    }

    public void AddFirmware(Firmware firmware)
    {
      FirmwarList.Add(firmware);
      SaveChanges();
    }

    public void RemoveFirmware(int id)
    {
      FirmwarList.Remove(FindFirmware(id));
      SaveChanges();
    }

    public bool FirmwareExists(Firmware firmware)
    {
      var items = FirmwarList.Where(item => item.Name == firmware.Name && item.Version == firmware.Version);
      return items.Count() != 0;
    }

  }

  public enum FirmwareType
  {
    None,
    Homie_ESP8266
  }

  [Table("Firmware")]
  public class Firmware
  {
    [JsonProperty("id")]
    public int ID { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("date_created")]
    public DateTime DateCreated { get; set; }
    [JsonProperty("type")]
    public FirmwareType Type { get; set; }
    [JsonProperty("disabled")]
    public bool Disabled { get; set; }
    [JsonIgnore]
    public byte[] Binary { get; set; }

    public string GetHashHex()
    {
      var hash = MD5.Create().ComputeHash(Binary);
      StringBuilder hex = new StringBuilder(hash.Length * 2);
      foreach (byte b in hash)
        hex.AppendFormat("{0:x2}", b);
      return hex.ToString();
    }

    public string Base64Firmware()
    {
      return Convert.ToBase64String(Binary);
    }
  }
}
