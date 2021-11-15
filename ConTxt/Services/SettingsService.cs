using System;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace WebBrowser
{
    abstract class SettingsService : IReloadable
    {
        private XmlDocument xml;
        private string settingsFilePath;
        public SettingsService(string SettingsFilePath){
            settingsFilePath = SettingsFilePath;
            reloadSettings();
        }
        private bool tryReadSettingFromFile(string settingKeyNameInXml, out XmlNode firstMatchingNode){
            firstMatchingNode = xml.GetElementsByTagName(settingKeyNameInXml.ToLower())[0];
            return firstMatchingNode != null;
        }
        private PropertyInfo[] getPropertiesOfGivenType(Type type){
            return this.GetType().GetProperties().Where<PropertyInfo>((PropertyInfo p) => p.PropertyType == type).ToArray();
        }
        private void reloadIntSettings(){

            foreach (PropertyInfo property in getPropertiesOfGivenType(typeof(int))){

                XmlNode firstMatchingNode;

                if (tryReadSettingFromFile(property.Name, out firstMatchingNode)){
                    try {
                        property.SetValue(
                            this,
                            Int32.Parse(
                                firstMatchingNode.InnerText
                            )
                        );
                    }
                    catch (Exception ex){
                        Console.WriteLine($"Failed to load inner text from tag '{property.Name}' in {settingsFilePath}. Is the tag name correct?");
                        Console.WriteLine($"The default value of '{property.GetValue(this)}' will be used.");
                        Console.WriteLine($"Error: {ex}");
                    }
                }
            }
        }
        private void reloadStringSettings(){

            foreach (PropertyInfo property in getPropertiesOfGivenType(typeof(string))){

                XmlNode firstMatchingNode;

                if (tryReadSettingFromFile(property.Name, out firstMatchingNode)){
                    try {
                        property.SetValue(
                            this,
                            firstMatchingNode.InnerText
                        );
                    }
                    catch (Exception ex){
                        Console.WriteLine($"Failed to load inner text from tag '{property.Name}' in {settingsFilePath}. Is the tag name correct?");
                        Console.WriteLine($"The default value of '{property.GetValue(this)}' will be used.");
                        Console.WriteLine($"Error: {ex}");
                    }
                }
            }
        }
        private void reloadStringListSettings(){

            foreach (PropertyInfo property in getPropertiesOfGivenType(typeof(List<string>))){

                XmlNode firstMatchingNode;
                
                if (tryReadSettingFromFile(property.Name, out firstMatchingNode)){
                    try {
                        foreach (XmlNode listEntry in firstMatchingNode.ChildNodes){
                            try{
                                List<string> propertyListObject = (List<string>) property.GetValue(this);
                                object[] valueToAdd = {listEntry.InnerText};
                                propertyListObject.GetType().GetMethod("Add").Invoke(propertyListObject, valueToAdd);
                            }
                            catch (Exception ex){
                                Console.WriteLine($"Failed to load inner text from a child tag of tag '{property.Name}' in {settingsFilePath}.");
                                Console.WriteLine($"Error: {ex}");
                            }
                        }
                    }
                    catch (Exception ex){
                        Console.WriteLine($"Failed to load child elements of tag '{property.Name}' in {settingsFilePath}. Is the tag name correct?");
                        Console.WriteLine($"The default value of '{property.GetValue(this)}' will be used.");
                        Console.WriteLine($"Error: {ex}");
                    }
                }
            }
        }
        public void reloadSettings(){

            xml = new XmlDocument();

            try{
                xml.Load(settingsFilePath);
            }
            catch{
                Console.WriteLine($"Failed to load file {settingsFilePath}.");
                Console.WriteLine("The default values will be used.");
                return;
            }

            reloadIntSettings();
            reloadStringSettings();
            reloadStringListSettings();
        }
    }
}