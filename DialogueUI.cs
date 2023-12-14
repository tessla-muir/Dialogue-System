using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Dialogue;
using TMPro;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;

namespace Game.UI
{
    public class DialogueUI : MonoBehaviour
    {
        PlayerConversant playerConversant;
        [SerializeField] TextMeshProUGUI speakerText;

        [SerializeField] Button button;
        [SerializeField] Sprite continueSprite;
        [SerializeField] Sprite skipSprite;
        [SerializeField] Sprite quitSprite;

        [SerializeField] GameObject textResponse;
        [SerializeField] Transform choiceRoot;
        [SerializeField] GameObject choicePrefab;

        [SerializeField] TextMeshProUGUI speakerName;
        [SerializeField] Image speakerImage;
        [SerializeField] Sprite playerSprite;
        [SerializeField] Sprite AISprite;

        private Coroutine displayTextCoroutine;
        private bool textFullyDisplayed = false;

        void Start()
        {
            playerConversant = GameObject.Find("Player").GetComponent<PlayerConversant>();
            playerConversant.onConversationUpdated += UpdateUI;
            button.onClick.AddListener(Next);


            // Initialize the UI and inactivate it
            UpdateUI();
            gameObject.SetActive(false);
        }

        // Continues dialogue, updating UI
        void Next()
        {
            // Display whole text if user prompts to do so
            if (!textFullyDisplayed)
            {
                StopCoroutine(displayTextCoroutine);
                speakerText.text = playerConversant.GetText();
                textFullyDisplayed = true;
                UpdateNextButton();
                return;
            }

            if (playerConversant.HasNext() && !playerConversant.IsChoosing())
            {
                playerConversant.Next();
            }
            else
            {
                // Exit Dialogue
                playerConversant.Quit();
                gameObject.SetActive(false);
            }
        }

        void UpdateUI()
        {
            if (!playerConversant.IsActive()) return;

            // Activate text UI or choice UI
            textResponse.SetActive(!playerConversant.IsChoosing());
            choiceRoot.gameObject.SetActive(playerConversant.IsChoosing());

            // Choice UI is displayed
            if (playerConversant.IsChoosing())
            {
                // Set player sprite & name
                speakerImage.sprite = playerSprite;
                speakerName.text = "Player";

                BuildChoiceList();
            }
            // Text UI is displayed for player
            else if (playerConversant.HasSingleChoice())
            {
                BuildTextResponse();
            }
            // Text UI is displayed for AI
            else
            {
                BuildTextResponse();
            }

            // Set sprite and name
            speakerName.text = playerConversant.GetConversantName();
            speakerImage.sprite = playerConversant.GetConversantSprite();
        }

        private void BuildTextResponse()
        {
            if (displayTextCoroutine != null)
            {
                StopCoroutine(displayTextCoroutine);
            }
            displayTextCoroutine = StartCoroutine(DisplayText());

            UpdateNextButton();
        }

        private void UpdateNextButton()
        {
            // Change look of button when at end of dialogue
            if (!textFullyDisplayed)
            {
                button.transform.GetChild(0).GetComponent<Image>().sprite = skipSprite;
            }
            else if (!playerConversant.HasNext())
            {
                button.transform.GetChild(0).GetComponent<Image>().sprite = quitSprite;
            }
            else
            {
                button.transform.GetChild(0).GetComponent<Image>().sprite = continueSprite;
            }
        }

        private void BuildChoiceList()
        {
            // Empty out choices
            // Possible performance delay - could keep track of choices and update accordingly instead
            foreach (Transform choice in choiceRoot)
            {
                Destroy(choice.gameObject);
            }

            // Fill out choices
            foreach (DialogueNode choice in playerConversant.GetChoices())
            {
                // Create choice button and set text
                GameObject choiceInstance = Instantiate(choicePrefab, choiceRoot);
                choiceInstance.GetComponentInChildren<TextMeshProUGUI>().text = choice.GetText();

                Button button = choiceInstance.GetComponentInChildren<Button>();
                button.onClick.AddListener(() =>
                {
                    playerConversant.SelectChoice(choice);
                });
            }
        }

        IEnumerator DisplayText()
        {
            textFullyDisplayed = false;
            speakerText.text = "";

            string text = playerConversant.GetText();

            int textIndex = 0;
            while (textIndex < text.Length)
            {
                if (text[textIndex] == '<')
                {
                    // Find closing >
                    int closingIndex = text.IndexOf('>', textIndex);
                    if (closingIndex == -1)
                    {
                        break;
                    }

                    // Instantly add text between tags
                    speakerText.text += text.Substring(textIndex, closingIndex - textIndex + 1);

                    // Advance past closing tag
                    textIndex = closingIndex + 1;
                }
                else
                {
                    // Delay each character
                    speakerText.text += text[textIndex];
                    textIndex++;

                    yield return new WaitForSeconds(0.05f);
                }
            }

            textFullyDisplayed = true;
            UpdateNextButton();
        }
    }
}
