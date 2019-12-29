# Visualization Force Directed Graph
Data visualization is used to analyze abstract patterns in data, but when visualizing large data, we can face clustering problems in graphs that make it resemble hairballs. 

A common way to solve this problem is to use a Force Directed Graph to arrange and connect the nodes to make it easily analyzable. The Force Directed Graph adds a force to each node based on how heavily connected each of the data points are. 

## Industry
The largest implication of a force directed graph is its use with the analysis of social media networks. According to Neo4j, a company whose mission enables organizations to unlock the business value of connections, influences, and relationships in data, the high density of connections in social media along with the relationship of each connection are the most crucial pieces of information for use in understanding each user.

## Problem Solutions and Interactions
The Force Directed Graph was created in Unity using C# scripts to parse and generate the graph. Data used to generate this graph is the Meetup Group Connections dataset from Kaggle. The following approach was used to solve the problems mentioned above:
* Using an algorithm to automatically place nodes - repulsing from the center to avoid the heavy clustering problem and contracting heavily weighted nodes towards each other.
* Coloring by density of members for each node - Pink(High Density) and Blue(Low Density).
* Allowing the user to explore the graph - Zoom features and orbiting the entire graph
* Selecting a specific node to pop up its information and highlighting only its connected nodes to improve readability.


<a href="https://www.youtube.com/watch?v=NCek6mg-cnU" target="_blank"><img src="https://github.com/storm-king/VisualizationForceDirectedGraph/blob/master/MediumSizeGraph.png" 
alt="Medium Size Graph" width="500" height="500" border="10" /></a>



