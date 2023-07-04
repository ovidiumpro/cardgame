using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CG.Cards;
using UnityEngine;

public class GameState : MonoBehaviour
{
    //works with logic
    //keeps a reference to evertything relevant in the game
    //current hand, current void, current deck size, last card drawn, etc
    //will provide later on a way to talk to the server, by sending and updating gamestate

    public static GameState Instance;
    //Players
    public List<Player> players = new List<Player>();

    //Hands
    public Dictionary<Player, List<CardLogic>> playerHands = new Dictionary<Player, List<CardLogic>>();

    //TableState
    public Dictionary<Player, List<CreatureLogic>> playerTables = new Dictionary<Player, List<CreatureLogic>>();

    //Voids
    public Dictionary<Player, List<CardLogic>> playerVoids = new Dictionary<Player, List<CardLogic>>();

    //Decks
    public Dictionary<Player, List<CardAsset>> playerDecks = new Dictionary<Player, List<CardAsset>>();

    //Drawn
    public Dictionary<Player, List<CardLogic>> playerDrawnCards = new Dictionary<Player, List<CardLogic>>();

    //Last things to happen
    public Dictionary<Player, CardLogic> lastPlayedCardForEachPlayer = new Dictionary<Player, CardLogic>();
    public Dictionary<Player, CreatureLogic> lastCreatureToDieForEachPlayer = new Dictionary<Player, CreatureLogic>();

    public PlayStackElement lastThingInTheStack = null;

    //sizes
    public int playStackSize = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindObjectsOfType<Player>().ToList();
    }

    public List<CardLogic> DrawCardsFromDeck(Player p, int nr)
    {

        //this will eventually talk to server to determine what cards are drawn.
        //will return the list of cards to be drawn
        List<CardAsset> cards = p.deck.getCards(nr);
        playerDecks[p] = p.deck.getDeck();
        // Convert CardAssets to CardLogics
        List<CardLogic> cardLogics = cards.Select(cardAsset => new CardLogic(cardAsset)).ToList();
        return cardLogics;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
