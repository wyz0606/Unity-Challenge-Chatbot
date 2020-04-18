using System.Collections;
using UnityEngine;
using TMPro;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication;
using IBM.Watson.Assistant.V1;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Watson.Assistant.V1.Model;
using IBM.Cloud.SDK.Utilities;

public class Support : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField userInput;
    [SerializeField]
    private TextMeshProUGUI chatbotResponse;
    
    [Header("Credentials")]
    [SerializeField]
    private string versionDate, apiKEY, serviceUrl, myWorkspaceId;

    [Header("Loading Animation")]
    public Animator loading;

    private string myWords;
    private string theResponses;
    private int id;
    private Authenticator authenticator;
    private AssistantService assistant;
    

    // Start is called before the first frame update
    void Start()
    {
        LogSystem.InstallDefaultReactors();
        StartCoroutine(CreateService());
    }

    private IEnumerator CreateService()
    {
        authenticator = new IamAuthenticator(apikey: apiKEY);

        //  Wait for tokendata
        while (!authenticator.CanAuthenticate())
            yield return null;

        assistant = new AssistantService(versionDate, authenticator);
        assistant.SetServiceUrl(serviceUrl);
        Log.Debug("Start()", "ASS Service connection made successfully");
        id = Runnable.Run(Communication("Hello"));
    }

    private IEnumerator Communication(string _myWords)
    {
        MessageResponse messageResult = null;

        //Send Message to Cloud and get the response
        assistant.Message(
            callback: (DetailedResponse<MessageResponse> response, IBMError error) =>
            {
                if (error == null)
                {
                    Log.Debug("ExampleCallback", "Response received: {0}", response.Response);
                }
                else
                {
                    Log.Debug("ExampleCallback", "Error received: {0}, {1}, {3}", error.StatusCode, error.ErrorMessage, error.Response);
                }
                messageResult = response.Result;
            },
            workspaceId: myWorkspaceId,
            input: new MessageInput()
            {
                Text = _myWords
            }

        );

        while (messageResult == null)
            yield return null;

        theResponses = messageResult.Output.Generic[0].Text;

        //Loading Animation
        chatbotResponse.text = "";
        loading.SetBool("isLoading", true);
        for (int i = 0; i < 60; i++)
        {
            yield return null;
        }
        loading.SetBool("isLoading", false);
        yield return new WaitForSeconds(0.5f); 

        //Show message from chatbot
        foreach(char c in theResponses.ToCharArray())
        {
            chatbotResponse.text += c;
            yield return null;
        }

    }



    public void Send()
    {
        myWords = userInput.text;
        if(myWords != null)
        {
            Runnable.Stop(id);
            id = Runnable.Run(Communication(myWords));
        }
        
    }
}
