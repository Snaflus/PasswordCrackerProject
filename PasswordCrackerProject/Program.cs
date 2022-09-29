using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Master_Client.Models;
using Master_Client.Password_Cracker.Utils;

Stopwatch stopwatch = Stopwatch.StartNew();

string filename_dictionary = "webster-dictionary.txt";
//string filename_dictionary = "webster-dictionary-reduced.txt";
string filename_passwords = "passwords_cleartext.txt";

Console.WriteLine("Master Client");

BlockingCollection<List<string>> chunks = new BlockingCollection<List<string>>();
ReadDictionaryAndCreateChunks(filename_dictionary);

List<string> resultsUsernames = new List<string>();
List<string> resultsPasswords = new List<string>();

List<string> resultsList = new List<string>();

List<string> partialResultList1 = new List<string>();
List<string> partialResultList2 = new List<string>();

Parallel.Invoke(
    () =>
{
    partialResultList1 = InstancedClient("localhost",4571);
},
() =>
{
    partialResultList2 = InstancedClient("10.200.130.39",4572);
}
);
resultsList.AddRange(partialResultList1);
resultsList.AddRange(partialResultList2);

//resultsList = InstancedClient(4570);
foreach (var i in resultsList)
{
    string[] splitArray = i.Split(" ");
    resultsUsernames.Add(splitArray[0]);
    resultsPasswords.Add(splitArray[1]);
    stopwatch.Stop();
    Console.WriteLine($"Cracking complete, time elapsed: {stopwatch.Elapsed}");
}

if (resultsUsernames.Count != 0 && !chunks.Any())
{
    PasswordFileHandler.WritePasswordFile(filename_passwords, resultsUsernames, resultsPasswords);
    Console.WriteLine($"Client printed cracked data to {filename_passwords}");
    Console.ReadKey();
}

List<string> InstancedClient(string ip, int port)
{
    List<string> userInfoList = new List<string>();
    TcpClient socket = new TcpClient(ip, port);

    NetworkStream ns = socket.GetStream();
    StreamReader reader = new StreamReader(ns);
    StreamWriter writer = new StreamWriter(ns);

    while (true)
    {
        string? input = reader.ReadLine();
        Console.WriteLine($"Client received: {input}, from port: {port}");

        if (input == "request new chunk")
        {
            if (chunks.Any())
            {
                var data = chunks.Take();
                writer.WriteLine(JsonSerializer.Serialize(data));
                writer.Flush();
                Console.WriteLine($"Client sent: {data.Count} lines");
            }
            else
            {
                writer.WriteLine("no chunks");
                socket.Close();
                return userInfoList;
            }
        }
        else
        {
            userInfoList.Add(input);
        }
    }
}


void ReadDictionaryAndCreateChunks(string filename)
{
    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))

    using (StreamReader dictionary = new StreamReader(fs))
    {
        int counter = 0;
        List<string> chunk = new List<string>();

        while (!dictionary.EndOfStream)
        {

            String dictionaryEntry = dictionary.ReadLine();
            counter++;
            if ((counter % 10000) != 0)
            {
                chunk.Add(dictionaryEntry);
            }
            else
            {
                chunks.Add(chunk);
                chunk = new List<string>();
            }
        }
        chunks.Add(chunk);
}
}
