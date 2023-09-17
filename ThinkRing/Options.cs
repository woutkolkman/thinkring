using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace ThinkRing
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType, colorType, lightningType;
        public static Configurable<Color> staticHaloColor;
        public static Configurable<bool> whiteLightning, sound, blink;
        public int curTab;

        public enum ActivateTypes
        {
            Dragging,
            Always,
            ToolsActive
        }

        public enum ColorTypes
        {
            Static,
            CharacterDarker,
            Character,
            RGB
        }

        public enum LightningTypes
        {
            None,
            RustyMachine,
            Oracle,
            MoreSlugcats
        }


        public Options()
        {
            activateType = config.Bind("activateType", defaultValue: ActivateTypes.Dragging.ToString(), new ConfigurableInfo("Halo is visible on this condition.", null, "", "Show when"));
            colorType = config.Bind("colorType", defaultValue: ColorTypes.Static.ToString(), new ConfigurableInfo("The color the halo is going to be.\nIf you choose Character, you might not see your slugcat's outlines at all times.", null, "", "Color type"));
            lightningType = config.Bind("lightningType", defaultValue: LightningTypes.RustyMachine.ToString(), new ConfigurableInfo("Type of lightning bolts.", null, "", "Lightning type"));
            staticHaloColor = config.Bind("staticHaloColor", defaultValue: new Color((50f/255f), 0f, (50f/255f), 1f), new ConfigurableInfo("Configured static color for halo. Black makes halo invisible, except in Pebbles' room.\nSet body color to #990099 for Psychic color.", null, "", ""));
            whiteLightning = config.Bind("whiteLightning", defaultValue: true, new ConfigurableInfo("Uncheck to make lightning the same color as the halo itself.", null, "", "White lightning"));
            sound = config.Bind("sound", defaultValue: true, new ConfigurableInfo("Uncheck to mute lightning.", null, "", "Sound"));
            blink = config.Bind("blink", defaultValue: true, new ConfigurableInfo("Slugcat closes eyes when dragging things with your mouse.", null, "", "Blink"));
        }


        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "General")
            };

            /**************** General ****************/
            curTab = 0;
            AddTitle();
            AddDivider(530f);

            float y = 533f;
            AddComboBox(activateType, new Vector2(150f, y -= 40f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddCheckbox(whiteLightning, new Vector2(350f, y));
            AddCheckbox(sound, new Vector2(350f, y -= 40f));
            AddCheckbox(blink, new Vector2(350f, y -= 40f));
            AddDivider(y -= 10f);

            AddComboBox(colorType, new Vector2(150f, y -= 40f), Enum.GetNames(typeof(ColorTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddColorPicker(staticHaloColor, new Vector2(350f, y -= (150f - 24f)));
            AddDivider(y -= 10f);

            AddComboBox(lightningType, new Vector2(150f, y -= 40f), Enum.GetNames(typeof(LightningTypes)), alH: FLabelAlignment.Left, width: 120f);
        }


        private void AddTitle()
        {
            OpLabel title = new OpLabel(new Vector2(150f, 560f), new Vector2(300f, 30f), Plugin.Name, bigText: true);
            OpLabel version = new OpLabel(new Vector2(150f, 540f), new Vector2(300f, 30f), $"Version {Plugin.Version}");

            Tabs[curTab].AddItems(new UIelement[]
            {
                title,
                version
            });
        }


        private void AddIcon(Vector2 pos, string iconName)
        {
            Tabs[curTab].AddItems(new UIelement[]
            {
                new OpImage(pos, iconName)
            });
        }


        private void AddCheckbox(Configurable<bool> option, Vector2 pos, Color? c = null)
        {
            if (c == null)
                c = Menu.MenuColorEffect.rgbMediumGrey;

            OpCheckBox checkbox = new OpCheckBox(option, pos)
            {
                description = option.info.description,
                colorEdge = (Color)c
            };

            OpLabel label = new OpLabel(pos.x + 40f, pos.y + 2f, option.info.Tags[0] as string)
            {
                description = option.info.description,
                color = (Color)c
            };

            Tabs[curTab].AddItems(new UIelement[]
            {
                checkbox,
                label
            });
        }


        private void AddKeyBinder(Configurable<KeyCode> option, Vector2 pos, Color? c = null)
        {
            if (c == null)
                c = Menu.MenuColorEffect.rgbMediumGrey;

            OpKeyBinder keyBinder = new OpKeyBinder(option, pos, new Vector2(100f, 30f), false)
            {
                description = option.info.description,
                colorEdge = (Color)c
            };

            OpLabel label = new OpLabel(pos.x + 100f + 16f, pos.y + 5f, option.info.Tags[0] as string)
            {
                description = option.info.description,
                color = (Color)c
            };

            Tabs[curTab].AddItems(new UIelement[]
            {
                keyBinder,
                label
            });
        }


        private void AddComboBox(Configurable<string> option, Vector2 pos, string[] array, float width = 80f, FLabelAlignment alH = FLabelAlignment.Center, OpLabel.LabelVAlignment alV = OpLabel.LabelVAlignment.Center)
        {
            OpComboBox box = new OpComboBox(option, pos, width, array)
            {
                description = option.info.description
            };

            Vector2 offset = new Vector2();
            if (alV == OpLabel.LabelVAlignment.Top) {
                offset.y += box.size.y + 5f;
            } else if (alV == OpLabel.LabelVAlignment.Bottom) {
                offset.y += -box.size.y - 5f;
            } else if (alH == FLabelAlignment.Right) {
                offset.x += box.size.x + 20f;
                alH = FLabelAlignment.Left;
            } else if (alH == FLabelAlignment.Left) {
                offset.x += -box.size.x - 20f;
                alH = FLabelAlignment.Right;
            }

            OpLabel label = new OpLabel(pos + offset, box.size, option.info.Tags[0] as string)
            {
                description = option.info.description
            };
            label.alignment = alH;
            label.verticalAlignment = OpLabel.LabelVAlignment.Center;

            Tabs[curTab].AddItems(new UIelement[]
            {
                box,
                label
            });
        }


        private void AddColorPicker(Configurable<Color> option, Vector2 pos)
        {
            OpColorPicker colorPicker = new OpColorPicker(option, pos)
            {
                description = option.info.description
            };

            Tabs[curTab].AddItems(new UIelement[]
            {
                colorPicker
            });
        }


        //based on https://github.com/SabreML/MusicAnnouncements/blob/master/src/MusicAnnouncementsConfig.cs
        private void AddDivider(float y)
        {
            OpImage dividerLeft = new OpImage(new Vector2(300f, y), "LinearGradient200");
            dividerLeft.sprite.SetAnchor(0.5f, 0f);
            dividerLeft.sprite.rotation = 270f;
            dividerLeft.sprite.height *= 1.6f;

            OpImage dividerRight = new OpImage(new Vector2(300f, y), "LinearGradient200");
            dividerRight.sprite.SetAnchor(0.5f, 0f);
            dividerRight.sprite.rotation = 90f;
            dividerRight.sprite.height *= 1.6f;

            Tabs[curTab].AddItems(new UIelement[]
            {
                dividerLeft,
                dividerRight
            });
        }
    }
}
