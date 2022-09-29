using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Master_Client.Models;
using Master_Client.Password_Cracker.Utils;

string filename_dictionary = "webster-dictionary.txt";
//string filename = "webster-dictionary-reduced.txt";
string filename_passwords = "passwords.txt";

Console.WriteLine("Master Client");

TcpClient socket = new TcpClient("localhost", 4570);

NetworkStream ns = socket.GetStream();
StreamReader reader = new StreamReader(ns);
StreamWriter writer = new StreamWriter(ns);

BlockingCollection<List<string>> chunks = new BlockingCollection<List<string>>();
ReadDictionaryAndCreateChunks(filename_dictionary);

List<UserInfoClearText> results = new List<UserInfoClearText>();

while (true)
{
    string? input = reader.ReadLine();
    Console.WriteLine($"Client received: {input}");
    string[] inputArray = input.Split(" ");
    results.Add(new UserInfoClearText(inputArray[0], inputArray[1]));

    if (input == "request new chunk")
    {
        var data = chunks.Take();
        writer.WriteLine(JsonSerializer.Serialize(data));
        writer.Flush();
        Console.WriteLine($"Client sent: {data.Count} lines");
    }


    //PasswordFileHandler.WritePasswordFile(filename_passwords,);

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
