using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CG.Cards;
using System.Linq;

public class Deck : MonoBehaviour
{

    public List<CardAsset> cards = new List<CardAsset>();

    void Awake()
    {
        cards.Shuffle();
    }
    public List<CardAsset> getCards(int nr)
    {
        // Calculate the actual number of cards to take.
        // If nr is greater than the number of cards, just take all cards.
        int cardsToTake = Mathf.Min(nr, cards.Count);

        // Take the cards from the beginning of the list.
        List<CardAsset> takenCards = cards.Take(cardsToTake).ToList();

        // Remove the taken cards from the original list.
        cards.RemoveRange(0, cardsToTake);

        return takenCards;
    }
    public List<CardAsset> getDeck() {
        return cards;
    }

}
