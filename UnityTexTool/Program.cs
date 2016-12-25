﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityTexTool.UnityEngine;
using System.IO;
using ImageMagick;
using System.Runtime.InteropServices;

namespace UnityTexTool
{
    public class AppMsg
    {
        public static string msg =
            "======Support Format====\n" +
            "Alpha8, ARGB4444, RGB565, RGBA8888, DXT1, DXT3, ETC1, ETC2_RGB, ETC2_RGBA8,\n"+
            "========================\n" +
            " Usage:\n" +
            "-I -info :show texture info only\n" +
            "-d -dump :dump texture\n" +
            "-c -compress :compress texture\n" +
            "-i -input <path> :input name\n" +
            "-o -output <path> :output png name\n" +
            "-r -resS <path> :use specific *.resS file path\n" +
            "\n";
    }
    class Program
    {
        

        [DllImport("user32.dll", EntryPoint = "MessageBox")]
        public static extern int MsgBox(IntPtr hwnd, string text, string caption, uint type);
        public static void ShowMsgBox(string msg)
        {
            MsgBox(IntPtr.Zero, msg, "UnityTexTool", 1);
        }

        static void AddEnvironmentPaths()
        {
            System.Environment.SetEnvironmentVariable("PATH", System.IO.Path.Combine(Environment.CurrentDirectory, @"Library\64bit") + ";"
                                                            , EnvironmentVariableTarget.Process);
            System.Environment.SetEnvironmentVariable("PATH", System.IO.Path.Combine(Environment.CurrentDirectory, @"Library\tool") + ";"
                                                            , EnvironmentVariableTarget.Process);
        }

        private static void ShowArgsMsg()
        {
            
            Console.WriteLine(AppMsg.msg);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input_name">png name</param>
        /// <param name="output_name">tex2D name</param>
        private static void PNG2Texture(string input_name, string output_name, string resSFilePath = "./")
        {
            
            byte[] dstTex2D = File.ReadAllBytes(output_name);
            Texture2D texture = new Texture2D(dstTex2D);
            if (texture.isTexture2D == false)
            {
                return;
            }
            if (texture.format == TextureFormat.Alpha8 ||
                texture.format == TextureFormat.ARGB4444 ||
                texture.format == TextureFormat.RGBA32 ||
                texture.format == TextureFormat.ETC_RGB4 ||
                texture.format == TextureFormat.ETC2_RGB ||
                texture.format == TextureFormat.ETC2_RGBA8 ||
                texture.format == TextureFormat.DXT5 ||
                texture.format == TextureFormat.DXT1)
            {
                ImageMagick.MagickImage im = new MagickImage(input_name);
                im.Flip();
                byte[] sourceData = im.GetPixels().ToByteArray(0, 0, im.Width, im.Height, "RGBA");
                byte[] outputData;
                Console.WriteLine("Reading:{0}\n Width:{1}\n Height:{2}\n Format:{3}\n", input_name, im.Width, im.Height, texture.format.ToString());
                Console.WriteLine("Converting...");
                TextureConverter.CompressTexture(texture.format, im.Width, im.Height, sourceData, out outputData, texture.bMipmap);
                if (outputData != null)
                {
                    if (texture.bHasResSData == true)
                    {
                        output_name = string.Format("{0}/{1}", resSFilePath, texture.resSName);

                    }
                    if (File.Exists(output_name))
                    {
                        Console.WriteLine("Writing...{0}", output_name);
                        using (FileStream fs = File.Open(output_name, FileMode.Open, FileAccess.ReadWrite))
                        {
                            fs.Seek(texture.dataPos, SeekOrigin.Begin);
                            fs.Write(outputData, 0, outputData.Length);
                            fs.Flush();
                        }
                        Console.WriteLine("File Created...");
                    }
                    else
                    {
                        Console.WriteLine("Error: file {0} not found", output_name);
                    }
                    

                }

            }




        }

        private static void Texture2PNG(string input_name, string output_name, string resSFilePath = "")
        {
            byte[] input = File.ReadAllBytes(input_name);
            Texture2D texture = new Texture2D(input, resSFilePath);
            if (texture.isTexture2D == false)
            {
                return;
            }
            Console.WriteLine("Reading: {0}\n Width: {1}\n Height: {2}\n Format: {3}\n Dimension: {4}\n Filter Mode: {5}\n Wrap Mode: {6}\n Mipmap: {7}",
                                input_name,
                                texture.width,
                                texture.height,
                                texture.format.ToString(),
                                texture.dimension.ToString(),
                                texture.filterMode.ToString(),
                                texture.wrapMode.ToString(),
                                texture.bMipmap);

            if (texture.format == TextureFormat.Alpha8 ||
                texture.format == TextureFormat.ARGB4444 ||
                texture.format == TextureFormat.RGBA32 ||
                texture.format == TextureFormat.ETC_RGB4 ||
                texture.format == TextureFormat.ETC2_RGB ||
                texture.format == TextureFormat.ETC2_RGBA8 ||
                texture.format == TextureFormat.DXT5 ||
                texture.format == TextureFormat.DXT1 
                )
            {
                MagickReadSettings settings = new MagickReadSettings();
                settings.Format = MagickFormat.Rgba;
                settings.Width = texture.width;
                settings.Height = texture.height;
                
                ImageMagick.MagickImage im = new MagickImage(texture.GetPixels(), settings);
                im.Flip();
                im.ToBitmap().Save(output_name);
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Unity Texture2D Dump tool. \nCreated by wmltogether --20161225");
            
            AddEnvironmentPaths();
            //TextureConverter.Test();
            
            string filename = null;
            string outputName = null;
            string resSFilePath = null; //resS数据包存储路径
            bool bDecompress = false;
            bool bCompress = false;
            bool bShowInfo = false;
            bool bShowHelp = false;

            if (args.Length == 0)
            {
                ShowArgsMsg();
                Program.ShowMsgBox(string.Format("Error: no args \n  Please use this program in console!\n"));

                return;
            }
            #region check args
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                    {
                        switch (args[i].TrimStart('-'))
                        {
                            case "h":
                            case "help":
                                bShowHelp = true;
                                break;
                            case "I":
                            case "info":
                                bShowInfo = true;
                                break;
                            case "d":
                            case "dump":
                                bDecompress = true;
                                break;
                            case "o":
                            case "output":
                                outputName = args[++i];
                                break;
                            case "c":
                            case "compress":
                                bCompress = true;
                                break;
                            case "i":
                            case "input":
                                filename = args[++i];
                                break;
                            case "r":
                            case "resS":
                                resSFilePath = args[++i];

                                break;

                        }
                    }
                }

            }
            #endregion
            if (bShowHelp)
            {
                ShowArgsMsg();
                return;
            }
            if (bShowInfo && (filename != null))
            {
                Texture2D texture = new Texture2D(File.ReadAllBytes(filename), resSFilePath);
                if (texture.isTexture2D == false)
                {
                    return;
                }
                Console.WriteLine("Reading: {0}\n Width: {1}\n Height: {2}\n Format: {3}\n Dimension: {4}\n Filter Mode: {5}\n "+
                                    "Wrap Mode: {6}\n Mipmap: {7}\n ResS Type : {8}\n Data Offset: {9:X8}",
                                    filename,
                                    texture.width,
                                    texture.height,
                                    texture.format.ToString(),
                                    texture.dimension.ToString(),
                                    texture.filterMode.ToString(),
                                    texture.wrapMode.ToString(),
                                    texture.bMipmap,
                                    texture.bHasResSData,
                                    texture.dataPos);
                return;
            }
            if (filename == outputName)
            {
                Console.WriteLine("Error: can't overwrite input file");
                return;
            }
            if ((filename != null) && (outputName != null))
            {
                if (bDecompress) Texture2PNG(filename, outputName, resSFilePath);
                if (bCompress) PNG2Texture(filename, outputName, resSFilePath);
            }
            

        }
    }
}
