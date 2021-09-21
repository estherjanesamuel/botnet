﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OCR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Read {
		public static async Task HandleReadAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			if (message.ReplyToMessage is null) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Apa yang mau diread? Untuk ngeread gambar, reply `/read` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else if (message.ReplyToMessage.Photo is null || message.ReplyToMessage.Photo.Length == 0) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Pesan ini tidak ada gambarnya\\. Untuk ngeread gambar, reply `/read` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else {
				using MemoryStream originalImageStream = new();
				Telegram.Bot.Types.File fileInfo = await botClient.GetInfoAndDownloadFileAsync(
					fileId: message.ReplyToMessage.Photo.OrderByDescending(photoSize => photoSize.Width).First().FileId,
					destination: originalImageStream,
					cancellationToken: cancellationToken);

				string textResult = await Reader.ReadImageAsync(originalImageStream.ToArray());

				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: textResult
						.Replace("\\", "\\\\", StringComparison.InvariantCultureIgnoreCase)
						.Replace(".", "\\.", StringComparison.InvariantCultureIgnoreCase)
						.Replace("|", "\\|", StringComparison.InvariantCultureIgnoreCase)
						.Replace("!", "\\!", StringComparison.InvariantCultureIgnoreCase)
						.Replace("[", "\\[", StringComparison.InvariantCultureIgnoreCase)
						.Replace("]", "\\]", StringComparison.InvariantCultureIgnoreCase)
						.Replace("(", "\\(", StringComparison.InvariantCultureIgnoreCase)
						.Replace(")", "\\)", StringComparison.InvariantCultureIgnoreCase)
						.Replace("{", "\\{", StringComparison.InvariantCultureIgnoreCase)
						.Replace("}", "\\}", StringComparison.InvariantCultureIgnoreCase)
						.Replace("_", "\\_", StringComparison.InvariantCultureIgnoreCase)
						.Replace("-", "\\-", StringComparison.InvariantCultureIgnoreCase)
						.Replace("=", "\\=", StringComparison.InvariantCultureIgnoreCase)
						.Replace("*", "\\*", StringComparison.InvariantCultureIgnoreCase)
						.Replace("#", "\\#", StringComparison.InvariantCultureIgnoreCase)
						.Replace("/", "\\/", StringComparison.InvariantCultureIgnoreCase)
						.Replace("`", "\\`", StringComparison.InvariantCultureIgnoreCase)
						.Replace("&", "&amp;", StringComparison.InvariantCultureIgnoreCase)
						.Replace("<", "&lt;", StringComparison.InvariantCultureIgnoreCase)
						.Replace(">", "&gt;", StringComparison.InvariantCultureIgnoreCase),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.ReplyToMessage.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}