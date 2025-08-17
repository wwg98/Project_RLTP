using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameTableManager : MonoBehaviour
{
    static GameTableManager _uniqueinstance;

    private Dictionary<int, CardData> cardDataDict = new Dictionary<int, CardData>();
    private List<int> cardIDList = new List<int>();
        
    public static GameTableManager _instance
    {
        get { return _uniqueinstance; }
    }

    void Awake()
    {
        if (_uniqueinstance == null)
        {
            _uniqueinstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        CardTableLoad();
    }

    public void CardTableLoad()
    {
        TextAsset json = Resources.Load<TextAsset>("Tables/CardTable");
        if (json != null)
        {
            Load(json.text);
        }
    }

    public void Load(string jsonData)
    {
        List<CardData> cards = JsonConvert.DeserializeObject<List<CardData>>(jsonData);
        cardDataDict = cards.ToDictionary(card => card.ID);
        cardIDList = cards.Select(card => card.ID).ToList();
    }

    public CardData GetCardDataByID(int id)
    {
        if (cardDataDict.TryGetValue(id, out var card))
        {
            return card;
        }
        Debug.LogWarning($"ID {id}에 해당하는 카드가 없습니다.");
        return null;
    }

    public List<int> GetAllCardIDs()
    {
        return new List<int>(cardIDList);
    }
}

