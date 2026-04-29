using Skyjo.Core.GameLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Skyjo.Core.Services
{
    public class SaveService
    {
        public void Save(GameState state, string path)
        {
            try
            {
                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(path, json);
            }
            catch (Exception)
            {
                // handle/log if needed
            }
        }

        public GameState Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            var json = File.ReadAllText(path);

            var state = JsonSerializer.Deserialize<GameState>(json);

            if (state == null)
                throw new Exception("Failed to deserialize game state");

            return state;
        }
    }
}
