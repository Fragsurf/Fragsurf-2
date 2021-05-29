using Fragsurf.Gamemodes.Tricksurf;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public class trickidfix : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var maps = new List<string>()
        {
            "tricks_parc_colore",
            "tricks_buck-wild",
            "tricks_shimmer"
        };

        foreach (var m in maps)
        {
            var githubPath = $"https://raw.githubusercontent.com/Fragsurf/Tricks/main/{m}.json";
            using var client = new WebClient();
            string s = client.DownloadString(githubPath);
            var d = JsonConvert.DeserializeObject<TrickData>(s);

            var idx = 0;
            foreach(var trick in d.tricks)
            {
                trick.id = idx;
                idx++;
            }

            d.tricks = d.tricks.OrderBy(x => x.id).ToList();

            var json = JsonConvert.SerializeObject(d, Formatting.Indented);
            var filePath = "C:\\NewTricks\\" + m + ".json";
            File.WriteAllText(filePath, json);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
