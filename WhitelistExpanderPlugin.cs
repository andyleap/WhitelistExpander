using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Compiler;
using VRage.Library.Collections;
using VRage.Plugins;
using System.Reflection;
using System.IO;
using Sandbox.Game.Components;
using Sandbox;

namespace WhitelistExpander
{
    public class WhitelistExpanderPlugin : IPlugin
    {
        public void Dispose()
        {
        }

        public void Init(object gameInstance)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyFolder = Path.GetDirectoryName(assemblyLocation);
            if(!Directory.Exists(Path.Combine(assemblyFolder, "ModWhitelist")))
            {
                Directory.CreateDirectory(Path.Combine(assemblyFolder, "ModWhitelist"));
            }
            string[] whitelistFiles = Directory.GetFiles(Path.Combine(assemblyFolder, "ModWhitelist"));
            MySandboxGame.Log.WriteLine("Started Expanding Whitelist");
            MySandboxGame.Log.IncreaseIndent();
            foreach(string file in whitelistFiles)
            {
                var filestream = new StreamReader(file);
                while (!filestream.EndOfStream)
                {
                    var whitelist = filestream.ReadLine();

                    string[] splitwhitelist = whitelist.Split('.');
                    var type = string.Join(".", splitwhitelist.Take(splitwhitelist.Count() - 1));
                    var member = splitwhitelist.Last();
                    var actualtype = GetTypeByName(type);

                    if (actualtype == null)
                    {
                        continue;
                    }
                    if (member == "*")
                    {
                        MySandboxGame.Log.WriteLine(string.Format("Allowing {0} on {1}", member, type));
                        if (IlChecker.AllowedOperands.ContainsKey(actualtype))
                        {
                            IlChecker.AllowedOperands[actualtype] = null;
                        }
                        else
                        {
                            IlChecker.AllowedOperands.Add(actualtype, null);
                        }
                        continue;
                    }
                    else
                    {
                        var property = actualtype.GetProperty(member);
                        if (property != null)
                        {
                            if (!IlChecker.AllowedOperands.ContainsKey(actualtype))
                            {
                                IlChecker.AllowedOperands.Add(actualtype, new List<MemberInfo>());
                            }
                            if (IlChecker.AllowedOperands[actualtype] != null)
                            {
                                MySandboxGame.Log.WriteLine(string.Format("Allowing {0} on {1}", member, type));
                                if (property.GetGetMethod() != null)
                                {
                                    IlChecker.AllowedOperands[actualtype].Add(property.GetGetMethod());
                                }
                                if (property.GetSetMethod() != null)
                                {
                                    IlChecker.AllowedOperands[actualtype].Add(property.GetSetMethod());
                                }
                                continue;
                            }
                        }
                        else
                        { 
                            var methods = actualtype.GetMember(member);
                            if (!IlChecker.AllowedOperands.ContainsKey(actualtype))
                            {
                                IlChecker.AllowedOperands.Add(actualtype, new List<MemberInfo>());
                            }
                            if (IlChecker.AllowedOperands[actualtype] != null && methods.Length > 0)
                            {
                                foreach (var memberinfo in methods)
                                {
                                    MySandboxGame.Log.WriteLine(string.Format("Allowing {0} on {1}", memberinfo, type));
                                }
                                IlChecker.AllowedOperands[actualtype].AddRange(methods);
                                continue;
                            }
                        }
                    }
                }
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("Finished Expanding Whitelist");
        }

        public void Update()
        {
        }

        public Type GetTypeByName(string Name)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].FullName == Name)
                    {
                        return assemblyTypes[j];
                    }
                }
            }
            return null;
        }
    }
}
