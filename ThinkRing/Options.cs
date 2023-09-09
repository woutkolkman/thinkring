using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace ThinkRing
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType;
        public static Configurable<Color> haloColor;
        public int curTab;

        public enum ActivateTypes
        {
            Dragging,
            Always
        }


        public Options()
        {
            activateType = config.Bind("activateType", defaultValue: ActivateTypes.Dragging.ToString(), new ConfigurableInfo("Halo is visible on this condition.", null, "", "Show when"));
            haloColor = config.Bind("haloColor", defaultValue: new Color(0.19607843f, 0f, 0.19607843f, 1f), new ConfigurableInfo("Configured static color for halo.", null, "", ""));
        }


        public override void Initialize()
        {
            const float startHeight = 520f;

            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "General")
            };

            /**************** General ****************/
            curTab = 0;
            AddTitle();
            float x = 100f;
            float y = startHeight;
            AddComboBox(activateType, new Vector2(x, y - 27f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 100f);
            AddColorPicker(haloColor, new Vector2(225f, y -= 150f));
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
    }
}
