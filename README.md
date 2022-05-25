# TIRPClo – Complete Time Intervals-Related Patterns Mining

## Introduction 
Welcome!

We are extremely happy to submit our paper – **_Complete Mining of Time Intervals-Related Patterns_** to the *ACM Transactions on Knowledge Discovery from Data* journal. 

The paper introduces the **TIRPClo** algorithm, which we are happy to make available online.

TIRPClo is an efficient time intervals mining algorithm which is capable of the complete mining of:
- The *entire set* of frequent *Time Intervals Related-Patterns (TIRPs)*

- The complete set of frequent *closed TIRPs*

This repository enables the complete reproducibility of our experimental results, 
and we are highly confident that it will contribute to future research in the field of time intervals mining.

## Repository Contents
The contents of this repository are as follows: 
- The implementation of the TIRPClo algorithm, implemented in Visual C#

- Implementations of the competitor state-of-the-art methods evaluated in the paper, including:
  - KarmaLego (Moskovitch and Shahar 2015)
  - CCMiner (Chen et al. 2016)
  - ZMiner (Lee at al. 2020)
  - VertTIRP (Mordvanyuk et al. 2021)
  
- All the time intervals datasets on which the methods' performance has been evaluated in the paper, including both the real-world and synthetic datasets

- Our random synthetic datasets generator 
	 
## Code
In the paper's evaluation TIRPClo is compared to the **KarmaLego**, **ZMiner**, and **VertTIRP** algorithms in the discovery of the _entire_ set of frequent TIRPs, 
while it is compared to the **CCMiner** algorithm in the discovery of the frequent _closed_ TIRPs.

### Entire Frequent TIRPs Discovery 
The implementations of the methods for the discovery of all the frequent TIRPs (i.e., TIRPClo, KarmaLego and ZMiner) are available [here](EntireTIRPs), including:
- **TIRPClo**: The code of the TIRPClo algorithm, implemented in Visual C#
- **KarmaLego**: The _original_ C# implementation of the KarmaLego algorithm, provided by (Moskovitch and Shahar 2015)
- **ZMiner**: A C# implementation of the ZMiner algorithm, which is a _step-by-step translation_ of the original [python implementation](https://github.com/zedshape/zminer)</ins> published by the authors (Lee et al. 2020)
- **VertTIRP**: A C# implementation of the VertTIRP algorithm, which is a _step-by-step translation_ of the original python implementation published by the authors (Mordvanyuk et al. 2021) 

### Frequent Closed TIRPs Discovery 
The implementations of the methods for the discovery of the frequent closed TIRPs (i.e., TIRPClo and CCMiner) are available [here](ClosedTIRPs), including:  
- **TIRPClo**: The code of the TIRPClo algorithm, implemented in Visual C#
- **CCMiner**: A Visual C# implementation of the CCMiner algorithm (Chen et al. 2016) 

### NOTE 
In order to conduct a performance evaluation which is as fair comparison as possible of all the compared methods, we made sure that:
- Language: All methods are implemented in the same programming language – i.e., C#

- Input:
  - The input format of all methods is identical (explained in [Datasets](#Datasets) in detail)
  - The input is read and parsed using the exact same code and objects

- Output:
  - The output format of all methods is identical (exaplined in [Output](#Output) in detail)
  - All methods write to the output file one TIRP at a time, using the exact same code and objects

## Datasets
- **_Location_**: All the datasets which have been used for TIRPClo's evaluation are available [here](Datasets) 

- **_Contents_**: 
  - Real-world datasets including all the publicly available time intervals datasets, as well as all the real-world datasets which have been used by the authors of the competitor methods' papers
    - ASL (Papapetrou et al. 2009)
    - Diabetes (Moskovitch and Shahar 2015)
    - Smart-home (Jakkula and Cook 2011)
    - ASL-BU, ASL-GT, Blocks, Auslan2, Context, Pioneer, and Skating (Mörchen and Fradkin 2010), and Hepatitis (Patel et al. 2008)
  - Synthetic datasets
    - ST1, ST2, CT1 and CT2

- **_Format_**: All datasets are in a *.csv* format, which contains the following information
  - Total number of entity records, i.e., number of Symbolic Time Intervals (STIs) series in the dataset
  - Two rows for each specific entity's series of STIs
    - The first row contains the entity ID
    - The second row contains the entity's ordered series of STIs, in which each STI is represented as a triplet of a start-time, a finish-time and a symbol; separated by commas. Successive STIs are separated by a semicolon

- **_Description_**: For each dataset, a *{dataset}.txt* file is supplied, which lists the dataset's main properties, summarized in Table 1 in the paper

- **_Note_**: For convenience, a copy of the datasets directory is also available under each algorithm's project directory in the path *{algorithm}/{algorithm}/bin/Debug/netcoreapp2.1/Datasets/*

## Synthetic Datasets Generator 
In order to examine the methods' performance in extreme scenarios which are not met by the real-world datasets (e.g., an extremely large number of STIs or symbol types), 
experiments were also conducted on several synthetic datasets. The synthetic datasets have been generated by our [synthetic datasets generator](SyntheticDatasetsGenerator). 

The generator requires the following seven input parameters (default values and a running example are 
supplied within the [DatasetsGenerator.cs](SyntheticDatasetsGenerator/SyntheticDatasetsGenerator/DatasetsGenerator.cs) file):
- _`{minE,maxE}`_ – lower and upper bounds for the total number of entities in the dataset. The number of entities is selected uniformly at random between _`minE`_ and _`maxE`_
- _`{minSTIs,maxSTIs}`_ – lower and upper bounds for the number of STIs within a single entity's STIs series. For each entity, its total number of STIs is selected uniformly at random between _`minSTIs`_ and _`maxSTIs`_
- _`maxDuration`_ – the maximal time duration of a single STI
- _`maxTimestamp`_ – the maximal timestamp at which a STI may start
- _`maxS`_ – the maximal number of symbol types

## Dependencies
- Visual Studio 2015/2017/2019

- .NET Core 2.1 Framework

## Running Instructions
The recommended way to execute each of the algorithms on a selected dataset is as follows:
1. Download the repository and install the dependencies

2. Open the desired algorithm's project in Visual Studio

3. Right-click the project directory in the solution explorer > Properties > Select ".NET Core 2.1" as the Target Framework > Save 

4. Set the following algorithm's execution parameters within the main function in the _RunAlgorithm.cs_ file to the desired values 
(default values are supplied, see [Example](#Example))
    - Number of entities in the dataset
    - Minimum vertical support percentage
    - Maximal gap value
    - Path to the dataset (without the *".csv"* suffix)

5. Run
 
## Output
As soon as the algorithm terminates, the TWO following output files are created within the same directory as the input dataset 
- The primary output file: *"{dataset name}-support-{minimum vertical support}-maxgap-{maximal gap}.txt"* – 
which stores the discovered frequent TIRPs, including their features (e.g., vertical support) and all of their specific instances
- Another file, named *"{primary output file name}-stats.txt"*, which contains the total execution duration in milliseconds

## Example
The following default parameters values are supplied within the _RunAlgorithm.cs_ file, which is available under each algorithm's project directory:
- _`number of entities=65`_
- _`minimum vertical support=50%`_
- _`maximal gap=30`_
- _`file path='Datasets/asl/asl'`_

Running the TIRPClo algorithm, for example, with this set of parameters values results in the discovered frequent TIRPs file 
*"ASL-support-50-maxgap-30.txt"*, as well as the *"ASL-support-50-maxgap-30.txt-stats.txt"* file, 
which contains the total time duration of the algorithm's execution. 
These files are created under the *TIRPClo/TIRPClo/bin/Debug/netcoreapp2.1/Datasets/asl/* directory.
 
## References

[1] Chen, Y. C., Weng, J. T. Y., & Hui, L. (2016). A novel algorithm for mining closed temporal patterns from interval-based data. Knowledge and Information Systems, 46(1), 151-183.

[2] Jakkula, V., & Cook, D. (2011, August). Detecting anomalous sensor events in smart home data for enhancing the living experience. In Workshops at the twenty-fifth AAAI conference on artificial intelligence.

[3] Lee, Z., Lindgren, T., & Papapetrou, P. (2020, August). Z-Miner: An Efficient Method for Mining Frequent Arrangements of Event Intervals. In Proceedings of the 26th ACM SIGKDD International Conference on Knowledge Discovery & Data Mining (pp. 524-534).

[4] Mörchen, F., & Fradkin, D. (2010, April). Robust mining of time intervals with semi-interval partial order patterns. In Proceedings of the 2010 SIAM international conference on data mining (pp. 315-326). Society for Industrial and Applied Mathematics.

[5] Mordvanyuk, N., López, B., & Bifet, A. (2021). vertTIRP: Robust and efficient vertical frequent time interval-related pattern mining. Expert Systems with Applications, 168, 114276.

[6] Moskovitch, R., & Shahar, Y. (2015). Fast time intervals mining using the transitivity of temporal relations. Knowledge and Information Systems, 42(1), 21–48.

[7] Papapetrou, P., Kollios, G., Sclaroff, S., & Gunopulos, D. (2009). Mining frequent arrangements of temporal intervals. Knowledge and Information Systems, 21(2), 133.

[8] Patel, D., Hsu, W., & Lee, M. L. (2008, June). Mining relationships among interval-based events for classification. In Proceedings of the 2008 ACM SIGMOD international conference on Management of data (pp. 393-404).
