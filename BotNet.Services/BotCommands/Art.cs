﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Craiyon;
using BotNet.Services.RateLimit;
using BotNet.Services.ThisXDoesNotExist;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BotNet.Services.BotCommands {
	public static class Art {
		private static readonly RateLimiter RANDOM_ART_RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		private static readonly RateLimiter GENERATED_ART_RATE_LIMITER = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(5));
		private static readonly HashSet<int> HANDLED_MESSAGE_IDS = new();
		public static async Task GetRandomArtAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						GENERATED_ART_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

						DateTime started = DateTime.Now;
						Message responseMessage = await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "Generating art...",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId);

#pragma warning disable CA2000 // Dispose objects before losing scope
						CancellationTokenSource cancellationTokenSource = new();
#pragma warning restore CA2000 // Dispose objects before losing scope

						_ = Task.Run(async () => {
							try {
								for (int i = 60; i > 0; i -= 10) {
									await Task.Delay(10000, cancellationTokenSource.Token);
									_ = Task.Run(async () => {
										try {
											await botClient.EditMessageTextAsync(
												chatId: message.Chat.Id,
												messageId: responseMessage.MessageId,
												text: $"Generating art... Please wait for approximately {i} seconds.",
												cancellationToken: cancellationTokenSource.Token);
										} catch { }
									}, cancellationTokenSource.Token);
								}
								await botClient.EditMessageTextAsync(
									chatId: message.Chat.Id,
									messageId: responseMessage.MessageId,
									text: "Generating art... Should be completed anytime soon.",
									cancellationToken: cancellationTokenSource.Token);
							} catch { }
						});

						_ = Task.Run(async () => {
							try {
								List<byte[]> images = await serviceProvider.GetRequiredService<CraiyonClient>().GenerateImagesAsync(commandArgument, CancellationToken.None);
								DateTime finished = DateTime.Now;
								TimeSpan duration = finished - started;

								List<(Stream Stream, int Id)> imageStreams = new();
								for (int i = 0; i < images.Count; i++) {
									byte[] image = images[i];
									Stream imageStream = new MemoryStream(image);
									imageStreams.Add((imageStream, i + 1));
								}
								try {
									try {
										cancellationTokenSource.Cancel();
										cancellationTokenSource.Dispose();
										await botClient.DeleteMessageAsync(
											chatId: message.Chat.Id,
											messageId: responseMessage.MessageId);
									} catch {
										// Resume when message has already been deleted
									}
									await botClient.SendMediaGroupAsync(
										chatId: message.Chat.Id,
										media: imageStreams.Select(imageStream => (IAlbumInputMedia)new InputMediaPhoto(new InputMedia(imageStream.Stream, $"IMG{imageStream.Id}.png"))),
										replyToMessageId: message.MessageId);
								} finally {
									foreach ((Stream imageStream, _) in imageStreams) {
										imageStream.Dispose();
									}
								}
							} catch {
								await botClient.EditMessageTextAsync(
									chatId: message.Chat.Id,
									messageId: responseMessage.MessageId,
									text: "<code>Could not generate art</code>",
									parseMode: ParseMode.Html
								);
							}
						});
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Anda belum mendapat giliran. Coba lagi {cooldown}.",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				}
			} else {
				try {
					RANDOM_ART_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

					byte[] image = await serviceProvider.GetRequiredService<ThisArtworkDoesNotExist>().GetRandomArtworkAsync(cancellationToken);
					using MemoryStream imageStream = new(image);

					await botClient.SendPhotoAsync(
						chatId: message.Chat.Id,
						photo: new InputOnlineFile(imageStream, "art.jpg"),
						cancellationToken: cancellationToken);
				} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Saya belum selesai melukis. Coba lagi {cooldown}.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}
	}
}
