namespace Share.Serialization;

public interface IAppJsonSerializer
{
    string Serialize<T>(T value);
    T? Deserialize<T>(string json);
}
