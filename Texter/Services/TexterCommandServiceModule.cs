using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;
using OpenAI_API;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System;
using EmojiOne;

namespace Texter.Services;

public sealed class TexterCommandServiceModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly CommandHandlerService _commandHandlerService;
    private readonly OpenAIAPI _api;


    public TexterCommandServiceModule(CommandHandlerService commandHandlerService)
    {
        _commandHandlerService = commandHandlerService;

        try
        {
            string jsonString = File.ReadAllText("config.json");
            JObject jsonObject = JObject.Parse(jsonString);
            string chatgptToken = (string)jsonObject["ChatgptToken"];
            _api = new OpenAIAPI(chatgptToken);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading config.json: {ex.Message}");
            throw;
        }
    }

    private async Task translateSetup(string text, bool show)
    {
        /// give instruction as System
        var chat = _api.Chat.CreateConversation();
        chat.AppendSystemMessage("You are a bot that turn emojis in to sentences to translate what the user it saying." +
                                "If the user does not send you any emojis tell them how to fix the input." +
                                "If the user does you emojis send back the sentence you translated in quotes then a newline with the input text the user sent");
        string output = EmojiOne.EmojiOne.AsciiToShortname(text);

        chat.AppendUserInput(output);
        string response = await chat.GetResponseFromChatbot();
        if (show)
            await RespondAsync("**" + response + "**\n" + text);
        else
            await RespondAsync("**" + response + "**\n");
    }

    private async Task urbanSetup(string text, bool show)
    {
        /// give instruction as System
        var chat = _api.Chat.CreateConversation();
        chat.AppendSystemMessage("You are a bot that translates sentences a user sends into urban dictionary slang" +
                                "Send back the sentence you translated in quotes." +
                                "if you see a swear word try to turn it pg, but if you can't skip it." +
                                "Use improper grammar in the translated sentence. ");
        chat.AppendUserInput(text);
        string response = await chat.GetResponseFromChatbot();
        Console.WriteLine("urban reply: " + response + "\n");

        if (show)
            await RespondAsync("**" + response + "**\n" + text);
        else
            await RespondAsync("**" + response + "**\n");
    }

    private async Task richSetup(string text, bool show)
    {
        /// give instruction as System
        var chat = _api.Chat.CreateConversation();
        chat.AppendSystemMessage("You are a bot that translates sentences a user sends into a sentence of higher learnning. " +
                                "You are very pretentious and only use scientific words, and act like a phliosopher." +
                                "if you see a swear word try to turn it pg, but if you can't skip it." +
                                "try to keep it short" +
                                "only use proper grammar in the translated sentence.");
        chat.AppendUserInput(text);
        string response = await chat.GetResponseFromChatbot();
        Console.WriteLine("rich reply: " + response + "\n");

        if (show)
            await RespondAsync("**" + response + "**\n" + text);
        else
            await RespondAsync("**" + response + "**\n");
    }

    [SlashCommand("emojis", "emojis to regular text")]
    public async Task emojiTranslate(string text, bool showUserText = false)
    {
        await translateSetup(text.ToString(), showUserText);
    }

    [SlashCommand("urban", "text to urban dictionary")]
    public async Task urbanTranslate(string text, bool showUserText = false)
    {
        await urbanSetup(text.ToString(), showUserText);
    }

    [SlashCommand("rich", "emojis to regular text")]
    public async Task richTranslate(string text, bool showUserText = false)
    {
        await richSetup(text.ToString(), showUserText);
    }
}
