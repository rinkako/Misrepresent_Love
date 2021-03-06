using System;
using System.Collections.Generic;
using System.IO;


namespace Yuri.YuriPacker
{
    /// <summary>
    /// <para>包管理类：负责资源包的封装、解封、寻址和维护的类</para>
    /// <para>她是一个静态类</para>
    /// </summary>
    public static class PackageUtils
    {
        /// <summary>
        /// 把指定资源封装为一个包
        /// 她是一个静态方法
        /// </summary>
        /// <param name="fileList">一个装有待打包数据路径的向量</param>
        /// <param name="saveFile">打包文件的保存路径</param>
        /// <param name="pak">指示封装的内容在运行时环境的字典键</param>
        /// <param name="sign">包的签名</param>
        /// <returns>操作成功与否</returns>
        public static bool pack(List<string> fileList, string saveFile, string pak, string sign)
        {
            try
            {
                if (fileList == null)
                {
                    return false;
                }
                // 开启输出流
                StreamWriter synWriter = new StreamWriter(saveFile + ".pst");
                FileStream pakFs = new FileStream(saveFile, FileMode.Create);
                BinaryWriter pakBw = new BinaryWriter(pakFs);
                // 获取文件长度
                int fileEncounter = fileList.Count;
                synWriter.WriteLine(String.Format("___SlyviaLyyneheym@{0}@{1}@{2}", fileEncounter, pak, sign));
                // 打包文件
                FileStream fs;
                BinaryReader fbr;
                long accCounter = 0;
                //保存每张Image为byte[]
                for (int i = 0; i < fileEncounter; i++)
                {
                    fs = new FileStream(fileList[i], FileMode.Open);
                    fbr = new BinaryReader(fs);
                    long fcount = 0;
                    while (fs.Position != fs.Length)
                    {
                        pakBw.Write(fbr.ReadByte());
                        fcount++;
                    }

                    string[] nameSplitItem = fileList[i].Split('\\');
                    synWriter.WriteLine(String.Format("{0}:{1}:{2}", nameSplitItem[nameSplitItem.Length - 1], accCounter, fcount));
                    accCounter += fcount;
                    fs.Close();
                    fs.Dispose();
                }
                synWriter.WriteLine("___SlyviaLyyneheymEOF");
                synWriter.Close();
                pakBw.Close();
                pakFs.Close();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 把包中的某个资源解压出来
        /// 她是一个静态方法
        /// </summary>
        /// <param name="packFile">包路径</param>
        /// <param name="getFile">资源名称</param>
        /// <param name="saveFile">解压目标路径</param>
        /// <returns>操作成功与否</returns>
        public static bool unpack(string packFile, string getFile, string saveFile)
        {
            try
            {
                StreamReader synReader = new StreamReader(packFile + ".pst");
                FileStream pakFs = new FileStream(packFile, FileMode.Open);
                BinaryReader pakBr = new BinaryReader(pakFs);
                FileStream extFs = new FileStream(saveFile, FileMode.Create);
                BinaryWriter extBr = new BinaryWriter(extFs);
                string[] synHeadSplitItem = synReader.ReadLine().Split('@');
                if (synHeadSplitItem[0] != "___SlyviaLyyneheym")
                {
                    return false;
                }
                int fileEncounter = Convert.ToInt32(synHeadSplitItem[1]);
                long filePointer = -1;
                long fileSize = -1;
                for (int t = 0; t < fileEncounter; t++)
                {
                    string[] lineitem = synReader.ReadLine().Split(':');
                    if (lineitem[0] == getFile)
                    {
                        filePointer = Convert.ToInt64(lineitem[1]);
                        fileSize = Convert.ToInt64(lineitem[2]);
                        break;
                    }
                }
                if (filePointer == -1 || fileSize == -1)
                {
                    return false;
                }
                pakFs.Seek(filePointer, SeekOrigin.Begin);
                for (int t = 0; t < fileSize; t++)
                {
                    extBr.Write(pakBr.ReadByte());
                }
                //pakFs.Seek(filePointer, SeekOrigin.Begin);
                //byte[] myData = new byte[fileSize];
                //for (int t = 0; t < fileSize; t++)
                //{
                //    myData[t] = pakBr.ReadByte();
                //}
                //for (int t = 0; t < fileSize; t++)
                //{
                //    extBr.Write(myData[t]);
                //}
                pakBr.Close();
                pakFs.Close();
                extBr.Close();
                extFs.Close();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 获得一个封包对象的字节序列
        /// </summary>
        /// <param name="packFile">包路径</param>
        /// <param name="resourceName">资源名称</param>
        /// <param name="offset">资源在包中的偏移量</param>
        /// <param name="length">资源字节数</param>
        /// <returns>资源的字节序列</returns>
        public static byte[] getObjectBytes(string packFile, string resourceName, long offset, long length)
        {
            FileStream pakFs = new FileStream(packFile, FileMode.Open);
            BinaryReader pakBr = new BinaryReader(pakFs);
            pakFs.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = pakBr.ReadByte();
            }
            return buffer;
        }

        /// <summary>
        /// 获取包里的文件列表
        /// 她是一个静态方法
        /// </summary>
        /// <param name="packFile">包路径</param>
        /// <returns>包含了包里所有文件名称的向量</returns>
        public static List<string> getPackList(string packFile)
        {
            List<string> outList = new List<string>();
            try
            {
                StreamReader synReader = null;
                if (packFile.EndsWith(".pst") == true)
                {
                    return outList;
                }
                else
                {
                    synReader = new StreamReader(packFile + ".pst");
                }
                string[] synHeadSplitItem = synReader.ReadLine().Split('@');
                if (synHeadSplitItem[0] != "___SlyviaLyyneheym")
                {
                    return null;
                }
                int fileEncounter = Convert.ToInt32(synHeadSplitItem[1]);
                for (int t = 0; t < fileEncounter; t++)
                {
                    outList.Add(synReader.ReadLine().Split(':')[0]);
                }
                return outList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
