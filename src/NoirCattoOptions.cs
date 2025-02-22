using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using JollyCoop;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using MoreSlugcats;
using UnityEngine;

namespace NoirCatto;

public class NoirCattoOptions : OptionInterface
{
    public readonly ManualLogSource Logger;
    public NoirCattoOptions(ManualLogSource loggerSource)
    {
        Logger = loggerSource;
    }
    

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };
        
    }

    public override void Update()
    {
    }
    
    //Thanks Henpe
    class BetterComboBox : OpComboBox
    {
        public BetterComboBox(ConfigurableBase configBase, Vector2 pos, float width, List<ListItem> list) : base(configBase, pos, width, list) { }

        public bool IsOpen;
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if(this._rectList != null && !_rectList.isHidden)
            {
                for (int j = 0; j < 9; j++)
                {
                    this._rectList.sprites[j].alpha = 1;
                }

                IsOpen = true;
            }
            else
            {
                IsOpen = false;
            }
        }
    }
    
    
}