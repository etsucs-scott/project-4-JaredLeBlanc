using Skyjo.Core.GameEngine;
using Skyjo.Core.GameLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkyjoAvaloniaApp.Services
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
            catch (Exception ex)
            {
                // For now, just rethrow or log
                throw new Exception("Error saving game", ex);
            }
        }

        public GameState Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Save file not found");

            try
            {
                var json = File.ReadAllText(path);

                var state = JsonSerializer.Deserialize<GameState>(json);

                if (state == null)
                    throw new Exception("Failed to deserialize game state");

                return state;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading game", ex);
            }
        }
    }
}
