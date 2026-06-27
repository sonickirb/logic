using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO.Compression;

public static class SaveSystem
{
    public static void SaveWorldData(string worldName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/worlds/" + worldName + ".wrld";

        using (FileStream zip = new FileStream(path, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Create))
            {
                ZipArchiveEntry wrldEntry = archive.CreateEntry("world");
                using (Stream stream = wrldEntry.Open())
                {
                    WorldData data = new WorldData();
                    formatter.Serialize(stream, data);
                }
            }
        }

        //FileStream stream = new FileStream(path, FileMode.Create);

        //WorldData data = new WorldData();

        //formatter.Serialize(stream, data);
        //stream.Close();

        Debug.Log("saved World to " + path);
    }

    public static WorldData LoadWorldData(string worldName)
    {
        string path = Application.persistentDataPath + "/worlds/" + worldName + ".wrld";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            //FileStream stream = new FileStream(path, FileMode.Open);

            WorldData data;

            using (FileStream zip = new FileStream(path, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry wrldEntry = archive.Entries[0];
                    using (Stream stream = wrldEntry.Open())
                        data = formatter.Deserialize(stream) as WorldData;
                }
            }

            //WorldData data = formatter.Deserialize(stream) as WorldData;
            //stream.Close();
            

            return data;
        } else
        {
            Debug.LogError("World file not found in " + path);
            return null;
        }
    }

    public static List<string> GetAllWorlds()
    {
        List<string> worldPaths = new List<string>();
        string worldsFolder = Application.persistentDataPath + "/worlds/";
        if (Directory.Exists(worldsFolder))
        {
            DirectoryInfo d = new DirectoryInfo(worldsFolder);
            foreach (var file in d.GetFiles("*.wrld"))
            {
                Debug.Log(file.FullName);
                worldPaths.Add(file.FullName);
            }
        } else
        {
            Directory.CreateDirectory(worldsFolder);
        }
        return worldPaths;
    }
}