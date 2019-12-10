using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ForceDirectedScript : MonoBehaviour
{
    public List<string[]> csvData = new List<string[]>();


    public List<Node> nodes;
    public float desiredConnectedNodeDistance = 0.075f;
    public float connectedNodeForce = 0.1f;
    public float disconnectedNodeForce = 0.1f;
    public float alpha = 1;
    public float friction = 0.75f;
    public float MAX_VELOCITY = 3.0f;
    public List<GameObject> allNodes = new List<GameObject>();
    public List<GameObject> allConnections = new List<GameObject>();
    public List<Color[]> allConnectionsColors = new List<Color[]>();

    public GameObject textPrefab;
    public TextMesh nodeDescription = new TextMesh();
    public List<GameObject> labels = new List<GameObject>();

    public float speed = 5f;

    public bool isCategorized = false;


    // Start is called before the first frame update
    void Start()
    {
        readData("Assets/Resources/smGroupsEdge.csv");

        nodes = new List<Node>();
        for (int i = 0; i < 20; i++)
        {
            Node nodeToAdd = new Node();
            Node childNodeToAdd = new Node();
            System.Random rnd = new System.Random();
            int randomLine = rnd.Next(1, 100);

            var lineOfRawData = csvData[i];

            //Create node if it doesn't exist
            bool isFound = false;
            int indexOfNode = 0;
            foreach(var node in nodes)
            {
                if((string)node.data[0] == lineOfRawData[4])
                {
                    isFound = true;
                    break;
                }
                indexOfNode++;
            }
            if(!isFound)
            {
                nodeToAdd.data.Add(lineOfRawData[4]);
                nodeToAdd.data.Add(lineOfRawData[6]);
                nodeToAdd.data.Add(lineOfRawData[8]);
                nodeToAdd.position = UnityEngine.Random.insideUnitSphere;
                nodeToAdd.velocity = Vector3.zero;
            }

            //Create child node if it doesn't exist
            bool isChildFound = false;
            int indexOfChild = 0;
            foreach (var node in nodes)
            {
                if ((string)node.data[0] == lineOfRawData[5])
                {
                    isChildFound = true;
                    break;
                }
                indexOfChild++;
            }
            if (!isChildFound)
            {
                childNodeToAdd.data.Add(lineOfRawData[5]);
                childNodeToAdd.data.Add(lineOfRawData[7]);
                childNodeToAdd.data.Add(lineOfRawData[9]);
                childNodeToAdd.position = UnityEngine.Random.insideUnitSphere;
                childNodeToAdd.velocity = Vector3.zero;
                if(!isFound)
                {
                    nodeToAdd.children.Add(childNodeToAdd);
                    nodeToAdd.childWeights.Add(Convert.ToInt32(lineOfRawData[3]));
                }
                else
                {
                    nodes[indexOfNode].children.Add(childNodeToAdd);
                    nodes[indexOfNode].childWeights.Add(Convert.ToInt32(lineOfRawData[3]));
                }
            }
            else
            {
                if(!isFound)
                {
                    nodeToAdd.children.Add(nodes[indexOfChild]);
                    nodeToAdd.childWeights.Add(Convert.ToInt32(lineOfRawData[3]));
                }
                else
                {
                    nodes[indexOfNode].children.Add(nodes[indexOfChild]);
                    nodes[indexOfNode].childWeights.Add(Convert.ToInt32(lineOfRawData[3]));
                }
            }


            //Add nodes if they are new
            if(!isFound && nodeToAdd.data.Count > 0)
            {
                nodes.Add(nodeToAdd);
            }
            if(!isChildFound && childNodeToAdd.data.Count > 0)
            {
                nodes.Add(childNodeToAdd);
            } 
            
        }

        foreach(var node in nodes)
        {
            node.setSize();
        }

        DrawSpheres();
        

    }

    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.black;
        labelStyle.fontSize = 30;

        GUI.Label(new Rect(Screen.width / 2, 10, 100, 100), "Meetup Group Connections", labelStyle);
        
    }

        // Update is called once per frame
        void Update()
    {
        ApplyGraphForce();
        int finishedMovingCounter = 0;
        foreach(var node in nodes)
        {
            Vector3 prevPosition = node.position;
            node.position += node.velocity * Time.deltaTime;

            if(prevPosition == node.position)
            {
                finishedMovingCounter++;
            }
        }

        if(finishedMovingCounter < nodes.Count)
        {
            DrawSpheres();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == "Sphere")
                {
                    foreach(var node in nodes)
                    {
                        if(hit.transform.position == node.position)
                        {
                            if(labels.Count > 0)
                            {
                                foreach(var o in labels)
                                {
                                    Destroy(o);
                                }
                                labels.Clear();
                            }

                            int i = 0;
                            foreach (var line in allConnections)
                            {
                                Color[] colorsOfLine = allConnectionsColors[i];
                                line.GetComponent<LineRenderer>().startColor = colorsOfLine[0];
                                line.GetComponent<LineRenderer>().startColor = colorsOfLine[1];
                                i++;
                            }

                            foreach (var line in allConnections)
                            {
                                int numberOfPositions = line.GetComponent<LineRenderer>().positionCount;
                                if (line.GetComponent<LineRenderer>().GetPosition(0) == node.position || line.GetComponent<LineRenderer>().GetPosition(numberOfPositions - 1) == node.position)
                                {
                                    
                                }
                                else
                                {
                                    line.GetComponent<LineRenderer>().startColor = new Color(1, 1, 1, 0.1f);
                                    line.GetComponent<LineRenderer>().endColor = new Color(1, 1, 1, 0.1f);
                                }
                            }

                            var t = Instantiate(textPrefab, node.position, Quaternion.identity);
                            nodeDescription = t.GetComponentInChildren<TextMesh>();
                            nodeDescription.transform.position = node.position;
                            nodeDescription.transform.rotation = Camera.main.transform.rotation;
                            nodeDescription.text = node.data[0] + System.Environment.NewLine + node.data[1] + System.Environment.NewLine + node.data[2];
                            labels.Add(t);
                        }
                    }
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            int i = 0;
            foreach (var line in allConnections)
            {
                Color[] colorsOfLine = allConnectionsColors[i];
                line.GetComponent<LineRenderer>().startColor = colorsOfLine[0];
                line.GetComponent<LineRenderer>().endColor = colorsOfLine[1];
                i++;
            }
        }

    }

    private void ApplyGraphForce()
    {
        if (alpha >= 0)
        {
            foreach (var node in nodes)
            {
                //Get the nodes that are not connected to current node
                List<Node> disconnectedNodes = new List<Node>();
                foreach (var nodeToCheck in nodes)
                {
                    if (node != nodeToCheck)
                    {
                        if (!node.children.Contains(nodeToCheck))
                        {
                            disconnectedNodes.Add(nodeToCheck);
                        }
                    }
                }

                //Contraction force
                int childWeightCounter = 0;
                foreach (var connectedNode in node.children)
                {
                    var difference = node.position - connectedNode.position;
                    var distance = (difference).magnitude;
                    var appliedForce = (connectedNodeForce * node.childWeights[childWeightCounter]) * Mathf.Log10(distance / desiredConnectedNodeDistance);

                    if(distance < desiredConnectedNodeDistance)
                    {
                        if(connectedNode.velocity.x < MAX_VELOCITY)
                        {
                            if (connectedNode.velocity.y < MAX_VELOCITY)
                            {
                                if(connectedNode.velocity.z < MAX_VELOCITY)
                                {
                                    connectedNode.velocity += appliedForce * alpha * Time.deltaTime * difference.normalized;
                                }
                                else
                                {
                                    connectedNode.velocity.z = MAX_VELOCITY;
                                }
                            }
                            else
                            {
                                connectedNode.velocity.y = MAX_VELOCITY;
                            }
                        }
                        else
                        {
                            connectedNode.velocity.x = MAX_VELOCITY;
                        }
                        
                    }
                    else
                    {
                        if (connectedNode.velocity.x > -MAX_VELOCITY)
                        {
                            if (connectedNode.velocity.y > -MAX_VELOCITY)
                            {
                                if (connectedNode.velocity.z > -MAX_VELOCITY)
                                {
                                    connectedNode.velocity -= appliedForce * alpha * Time.deltaTime * difference.normalized;
                                }
                                else
                                {
                                    connectedNode.velocity.z = -MAX_VELOCITY;
                                }
                            }
                            else
                            {
                                connectedNode.velocity.y = -MAX_VELOCITY;
                            }
                        }
                        else
                        {
                            connectedNode.velocity.x = -MAX_VELOCITY;
                        }
                        
                    }
                    childWeightCounter++;
                }

                //Repulsion force
                foreach (var disconnectedNode in disconnectedNodes)
                {
                    var difference = node.position - disconnectedNode.position;
                    var distance = (difference).magnitude;
                    if (distance != 0)
                    {
                        var appliedForce = (disconnectedNodeForce / Mathf.Pow(distance, 2));
                        if (disconnectedNode.velocity.x < MAX_VELOCITY)
                        {
                            if (disconnectedNode.velocity.y < MAX_VELOCITY)
                            {
                                if (disconnectedNode.velocity.z < MAX_VELOCITY)
                                {
                                    disconnectedNode.velocity += appliedForce * alpha * Time.deltaTime * difference.normalized;
                                }
                                else
                                {
                                    disconnectedNode.velocity.z = MAX_VELOCITY;
                                }
                            }
                            else
                            {
                                disconnectedNode.velocity.y = MAX_VELOCITY;
                            }
                        }
                        else
                        {
                            disconnectedNode.velocity.x = MAX_VELOCITY;
                        }
                        
                    }
                }
            }
            alpha -= 0.3f;
        }
        else
        {
            foreach (var node in nodes)
            {
                if(node.velocity.x > MAX_VELOCITY)
                {
                    node.velocity.x = MAX_VELOCITY;
                }
                else if(node.velocity.x > 0)
                {
                    node.velocity.x -= friction;
                }
                else
                {
                    node.velocity.x = 0;
                }

                if (node.velocity.y > MAX_VELOCITY)
                {
                    node.velocity.y = MAX_VELOCITY;
                }
                else if (node.velocity.y > 0)
                {
                    node.velocity.y -= friction;
                }
                else
                {
                    node.velocity.y = 0;
                }

                if (node.velocity.z > MAX_VELOCITY)
                {
                    node.velocity.z = MAX_VELOCITY;
                }
                else if (node.velocity.z > 0)
                {
                    node.velocity.z -= friction;
                }
                else
                {
                    node.velocity.z = 0;
                }

            }
        }
    }

    void DrawSpheres()
    {
        if (allNodes.Count != 0)
        {
            foreach (GameObject o in allNodes)
            { 
                Destroy(o);
            }
            allNodes.Clear();
        }

        if (allConnections.Count != 0)
        {
            foreach (GameObject o in allConnections)
            {
                Destroy(o);
            }
            allConnections.Clear();
            allConnectionsColors.Clear();
        }


        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = node.position;
                sphere.transform.localScale = new Vector3(node.nodeSize, node.nodeSize, node.nodeSize);
                Material nodeMaterial = new Material(Shader.Find("VertexLit"));
                nodeMaterial.color = node.colorOfNode;
                sphere.GetComponent<Renderer>().material = nodeMaterial;
                sphere.gameObject.SetActive(true);
                allNodes.Add(sphere);


                foreach (var connectedNode in node.children)
                {
                    var line = new GameObject();
                    line.name = "Connection";
                    var lr = line.AddComponent<LineRenderer>();
                    lr.SetPosition(0, node.position);
                    lr.SetPosition(1, connectedNode.position);
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    Color colorOfStart = node.colorOfNode;
                    colorOfStart.a = 0.5f;
                    lr.startColor = colorOfStart;
                    Color colorOfEnd = connectedNode.colorOfNode;
                    colorOfEnd.a = 0.2f;
                    lr.endColor = colorOfEnd;
                    lr.startWidth = 0.05f;
                    lr.endWidth = 0.2f;
                    allConnections.Add(line);
                    Color[] colorsOfLine = { colorOfStart, colorOfEnd };
                    allConnectionsColors.Add(colorsOfLine);
                }
            }
        }
    }



    public void readData(string filename)
    {
        StreamReader file = new StreamReader(filename);
        string line;
        while ((line = file.ReadLine()) != null)
        {
            string[] line_data = line.Split(',');
            csvData.Add(line_data);
        }
    }
}



public class Node
{
    public Vector3 position = new Vector3();
    public Vector3 velocity = new Vector3();
    public List<Node> children = new List<Node>();
    public List<int> childWeights = new List<int>();
    public float nodeSize; // = UnityEngine.Random.Range(0.125f, 0.75f);
    public Color colorOfNode;
    public ArrayList data = new ArrayList();

    public Node()
    {
        nodeSize = 0.0f;
    }

    public void setSize()
    {
        if (data.Count > 0)
        {
            Debug.Log(data[1]);
            int memberCount = Convert.ToInt32(data[1]);
            nodeSize = ((float)memberCount / (float)6331) * 0.625f + 0.125f;
        }

        if (nodeSize < 0.25f)
        {
            colorOfNode = new Color(0.0f, 0.1f, 0.9f);
        }
        else if (nodeSize >= 0.25f && nodeSize < 0.375f)
        {
            colorOfNode = new Color(0.18f, 0.07f, 0.84f);
        }
        else if (nodeSize >= 0.375f && nodeSize < 0.5f)
        {
            colorOfNode = new Color(0.37f, 0.05f, 0.79f);
        }
        else if (nodeSize >= 0.5f && nodeSize < 0.625f)
        {
            colorOfNode = new Color(0.56f, 0.02f, 0.74f);
        }
        else if (nodeSize >= 0.625f)
        {
            colorOfNode = new Color(0.75f, 0.0f, 0.69f);
        }
    }

}
