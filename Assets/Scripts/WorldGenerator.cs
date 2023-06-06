using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : NetworkBehaviour
{
    private int _randomNumber;
    private int _tileNumber = 1;

    private float _horizonMultiplier;
    private float _verticalMultiplier;
    private float _oddHorizonOffset;
    private List<int[]> _citySpot = new List<int[]>();

    private GameObject _createdTile;

    private static WorldManager s_worldManager;

    [SerializeField] private GameObject _city;
    [SerializeField] private GameObject _grass;
    [SerializeField] private GameObject _forest;
    [SerializeField] private GameObject _rock;
    [SerializeField] private GameObject _water;
    [SerializeField] private GameObject _desert;

    [SerializeField] private  Tilemap _tileMap;
    [SerializeField] private  Tile _cityTile;
    [SerializeField] private  Tile _grassTile;
    [SerializeField] private  Tile _forestTile;
    [SerializeField] private  Tile _rockTile;
    [SerializeField] private  Tile _waterTile;
    [SerializeField] private  Tile _desertTile;

    private Dictionary<string, Tile> _mapDict;

    private void Start()
    {

        s_worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        if(IsHost)
        {
            // Get mapwidth and mapheight
            int mapWidth = s_worldManager.GetMapWidth();
            int mapHeight = s_worldManager.GetMapHeight();
            
            //Get amount of players and reserve city spots
            int amountOfPlayers = s_worldManager.GetAmountOfPlayers();

            // Here we are getting an offset so that the hexes will touch each other.
            _horizonMultiplier = _grass.GetComponent<Renderer>().bounds.size.x;
            _verticalMultiplier = _grass.GetComponent<Renderer>().bounds.size.y * 0.75f;

            _oddHorizonOffset = _grass.GetComponent<Renderer>().bounds.size.x / 2;

            // Here we check how many players will play and where the cities will spawn.
            int horizontalDistance;
            int horizontalDistanceR2;
            int horizontalDistanceR3;
            int verticalDistance;
            switch(amountOfPlayers)
            {
                case 1:
                    int[] coordinatesCity = new int[] {0,0};
                    _citySpot.Add(new int[] {0,0});
                    break;
                case > 1 and < 4:
                    horizontalDistance = Mathf.FloorToInt(mapWidth / (amountOfPlayers + 1));
                    
                    AddTocitySpot(amountOfPlayers,  horizontalDistance, 0);
                    break;
                case 4:
                    // There's gonna be 2 in one row, so we'll devide by 3 so the cities won't be at the border.
                    horizontalDistance = Mathf.FloorToInt(mapWidth / 3);
                    
                    // The same for the vertical ones.
                    verticalDistance = Mathf.FloorToInt(mapHeight/3);
                    
                    AddTocitySpot(amountOfPlayers, horizontalDistance, -verticalDistance,
                                            horizontalDistance, -verticalDistance + verticalDistance + verticalDistance);
                    break;
                case > 4 and < 7:
                    // Horizontal: here's gonna be 3 in row one and 2 or 3 in row 2.
                    // This means we'll divide the first row by 4 and the second by amountofplayers - 2;
                    horizontalDistance = Mathf.FloorToInt(mapWidth / 4);
                    horizontalDistanceR2 = Mathf.FloorToInt(mapWidth / (amountOfPlayers - 2));

                    // Here there  will be 2 rows so we'll devide by 3
                    verticalDistance = Mathf.FloorToInt(mapHeight / 3);

                    AddTocitySpot(amountOfPlayers, horizontalDistance, -verticalDistance,
                                            horizontalDistanceR2, -verticalDistance + verticalDistance + verticalDistance);
                    break;
                case > 6 and < 10:
                    // Horizontal calculations first two rows divide by 4 for a bufer at each side
                    // third row devided by amountofplayers - 5 (three for each row (so -6) + 1 to make -5))
                    horizontalDistance = Mathf.FloorToInt(mapWidth / 4);
                    horizontalDistanceR2 = Mathf.FloorToInt(mapWidth / 4);
                    horizontalDistanceR3 = Mathf.FloorToInt(mapWidth / (amountOfPlayers - 5));

                    // We'll have three rows so we'll divide by 4
                    verticalDistance = Mathf.FloorToInt(mapHeight / 4);

                    AddTocitySpot(amountOfPlayers, horizontalDistance, -verticalDistance,
                                            horizontalDistanceR2, -verticalDistance + verticalDistance,
                                            horizontalDistanceR3, -verticalDistance + verticalDistance + verticalDistance);
                    break;

            }
            // Here we are going to generate the world randomly.
            // For each row we generate the map from left to right.
            for(int height = -(mapHeight/2); height < (mapHeight/2); height++)
            {
                for(int width = -(mapWidth/2); width < (mapWidth/2); width++)
                {
                    int[] checkData = new int[] {width, height};
                    bool buildCity = false;

                    foreach(int[] item in _citySpot)
                    {
                        if(item[0] == checkData[0] && item[1] == checkData[1])
                        {
                            buildCity = true;
                        }
                    }

                    if (buildCity)
                    {
                        CreateTile(_city, width, height, "cityTile");
                        buildCity = false;
                    }
                    else
                    {
                        // We are going to generate a random number that we can use to get a random tile
                        _randomNumber = Random.Range(0, 11);
                        // To have more grass and water we are going to give them more numbers
                        switch(_randomNumber)
                        {
                            // If the number is 1, 2 or 3 or lower do grass, this gives 30% chance of grass
                            case < 4:
                                CreateTile(_grass, width, height, "grassTile");
                                break;
                            // If the number is 4, 5 or 6 the result will be forest. thus a 30% chance
                            case > 3 and < 7:
                                CreateTile(_forest, width, height, "forestTile");
                                break;
                            // If the number is 7 or 8 the result will be water, thus a 20% chance
                            case > 6 and < 9:
                                CreateTile(_water, width, height, "waterTile");
                                break;
                            // If the number is a 9 it will do rocks thus 10% chance for rocks
                            case > 8 and <10:
                                CreateTile(_rock, width, height, "rockTile");
                                break;
                            // If the number is a 10 it will do desert thus 10% chance for desert
                            case >9:
                                CreateTile(_desert, width, height, "desertTile");
                                break;
                        }
                        // CreateTile(rock, width, height, "rockTile");
                    }
                }
            }
            s_worldManager.DistributeStartingAreaServerRpc();
        }
    }

    private void CreateTile(GameObject tile, int width, int height, string map)
    {
        if(height%2==0)
        {
            _createdTile = Instantiate(tile, new Vector3(width * _horizonMultiplier, height * _verticalMultiplier, -1f), Quaternion.identity);
        }
        else{
            _createdTile  = Instantiate(tile, new Vector3(width * _horizonMultiplier + _oddHorizonOffset, height * _verticalMultiplier, -1f), Quaternion.identity);
        }
        _createdTile.GetComponent<NetworkObject>().Spawn();
        WorkWithTileClientRpc(_createdTile.name, _tileNumber, map, width, height);
        _tileNumber++;
    }

    [ClientRpc]
    private void WorkWithTileClientRpc(string _createdTileName, int client_TileNumber, string map, int width, int height)
    {
        GameObject client_CreatedTile = GameObject.Find(_createdTileName);
        s_worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        Tilemap tileMap = GameObject.Find("HexagonalMap").GetComponent<Tilemap>();
        _mapDict = new Dictionary<string, Tile>  {{"cityTile", _cityTile},
                                                  {"grassTile", _grassTile},
                                                  {"forestTile", _forestTile},
                                                  {"rockTile", _rockTile},
                                                  {"waterTile", _waterTile},
                                                  {"desertTile", _desertTile}};

        // Change the tile name to somesthing we can more easily debug if needed.
        // Add it to a city list so that the world manager knows where they are so they can be used later on
        // Add the tile to an dictionary so we can get to the gameobject more easily later on
        if(client_CreatedTile != null)
        {
            client_CreatedTile.name = client_TileNumber.ToString();
            if(client_CreatedTile.tag == "City")
                {
                    Debug.Log("CITY");
                    s_worldManager.AddToCityList(client_CreatedTile);
                }
            s_worldManager.AddToTileDictionary(client_CreatedTile.name, client_CreatedTile);
            tileMap.SetTile(new Vector3Int(width, height, 0), _mapDict[map]);
        }
    }

    private void AddTocitySpot( int amountOfPlayers, 
                                int horizontalDistanceR1, int verticalDistanceR1,
                                int horizontalDistanceR2 = 0, int verticalDistanceR2 = 0,
                                int horizontalDistanceR3 = 0, int verticalDistanceR3 = 0)
    {
        // Add the city spots depended on the amount of players. This way the cities are always on the same spot.
        // This is good for debugging and such. Later we will have randomly generated locations for the cities.
        switch(amountOfPlayers)
        {
            case < 4:
                for(int player = 0; player < amountOfPlayers; player++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR1 + player * horizontalDistanceR1, verticalDistanceR1});
                }
                break;
            case 4:
                _citySpot.Add(new int[] {-horizontalDistanceR1, verticalDistanceR1});
                _citySpot.Add(new int[] {-horizontalDistanceR1 + horizontalDistanceR1 * 2, verticalDistanceR1});
                _citySpot.Add(new int[] {-horizontalDistanceR2 , verticalDistanceR2});
                _citySpot.Add(new int[] {-horizontalDistanceR2 + horizontalDistanceR2 * 2, verticalDistanceR2});
                break;
            case > 4 and < 7:
                for(int player = 0; player < 3; player++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR1 + player * horizontalDistanceR1, verticalDistanceR1});
                }
                for(int secondRowPlayer = 0; secondRowPlayer < amountOfPlayers - 3; secondRowPlayer++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR2 + secondRowPlayer * horizontalDistanceR2,
                                                                verticalDistanceR2});
                }
                break;
            case 7:
                for(int player = 0; player < 3; player++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR1 + player * horizontalDistanceR1, verticalDistanceR1});
                }
                for(int secondRowPlayer = 0; secondRowPlayer < 3; secondRowPlayer++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR2 + secondRowPlayer * horizontalDistanceR2,
                                                                verticalDistanceR2});
                }
                for(int thirdRowPlayer = 0; thirdRowPlayer < amountOfPlayers - 6; thirdRowPlayer++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR3 + horizontalDistanceR3, verticalDistanceR3});
                }
                break;
            case > 7 and < 10:
                for(int player = 0; player < 3; player++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR1 + player * horizontalDistanceR1, verticalDistanceR1});
                }
                for(int secondRowPlayer = 0; secondRowPlayer < 3; secondRowPlayer++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR2 + secondRowPlayer * horizontalDistanceR2,
                                                                verticalDistanceR2});
                }
                for(int thirdRowPlayer = 0; thirdRowPlayer < amountOfPlayers - 6; thirdRowPlayer++)
                {
                    _citySpot.Add(new int[] {-horizontalDistanceR3 + thirdRowPlayer * horizontalDistanceR3,
                                                                verticalDistanceR3});
                }
                break;
        }
        //return _citySpot;
    }
}
