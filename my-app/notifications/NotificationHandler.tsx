import messaging from "@react-native-firebase/messaging";
import * as Notifications from "expo-notifications";
import { useEffect } from "react";
import { Platform, ToastAndroid } from "react-native";
import { StreamChat } from "stream-chat";
import { chatApiKey } from "../chatConfig";
import { useChatContext } from "../ChatContext";

export function NotificationHandler() {
  const { getUserFromLocalStorage } = useChatContext();
  const client = StreamChat.getInstance(chatApiKey);

  useEffect(() => {

    messaging().onMessage(async (remoteMessage) => {
        //at this point we might want to display a toast, maybe add something to the notification tray, filter based on what channel screen we are on etc.
        console.log(remoteMessage);
        if (Platform.OS === "android") {
          ToastAndroid.show("Received Message", ToastAndroid.SHORT);
        }
    })

        //this function is run when the app is minimised or closed
    messaging().setBackgroundMessageHandler(async (remoteMessage) => {
      //this is run each time the notification is received
      const response = await getUserFromLocalStorage();
      if (!response) {
        return;
      }

      //this will only work on android, on ios you should grab from the notification template
      //https://getstream.io/chat/docs/sdk/reactnative/guides/push-notifications-v2/
      const { user, accessToken } = response;
      client._setToken(
        {
          id: user.id,
        },
        accessToken
      );

      try {
        const message = await client.getMessage(
          remoteMessage.data?.id as never as string
        );

        // You can also display a local notification here if needed
        await Notifications.scheduleNotificationAsync({
          content: {
            title: `New Message from ${message.message.user?.username}`,
            body: message.message.text,
          },
          trigger: null, // This triggers the notification immediately
        });
      } catch (error) {
        console.error(error);
        return;
      }
    });


  }, []);

  return null;
}
