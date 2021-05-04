using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TD.Server {
    public class AiPlayer : Player {
        [Space(12)]
        [Header("AI Stuffs")]
        public static string[] usernameStrings = { "Carl", "DareD33vil", "Sherlock", "Tyranner", "asdasd", "majom", "gamervil", "Vippo", "Lida", "Tresh55", "Hrgxh", "EmperorJax", "rtx", "Clement", "IBeatYa", "lovboy", "Martha", "Daddy", "kitkat", "vacuumCleaner", "D3velop3r", "poppy", "404", "Weed4Life", "Tigerrr", "ForTheHorde", "lol", "Monster", "lusty", "Sue1", "Misi", "Player24", "Player1", "Mr.Smith", "+-+", "I <3 You", "DckFace", "Deku", "Musketiir", "voldi", "lukeskyW", "12345", "Dragasag" };

        protected override void Start() {
            base.Start();

        }

        // Update is called once per frame
        private void FixedUpdate() {

        }


    }
}