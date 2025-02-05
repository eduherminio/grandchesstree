# The Grand Chess Tree
The grand chess tree is a public distributed effort to traverse the depths of the game of chess. The project started as a result of the enjoyment I found in the early days of working on [Saplings](https://github.com/Timmoth/Sapling) move generator. 


```
| depth | nodes            | captures        | enpassants   | castles       | promotions  | direct_checks  | single_discovered_checks | direct_discovered_checks | double_discovered_check | total_checks   | direct_mates | single_discovered_mates | direct_discoverd_mates | double_discoverd_mates | total_mates  |
|-------|------------------|-----------------|--------------|---------------|-------------|----------------|--------------------------|--------------------------|-------------------------|----------------|--------------|-------------------------|------------------------|------------------------|--------------|
| 0     | 1                | 0               | 0            | 0             | 0           | 0              | 0                        | 0                        | 0                       | 0              | 0            | 0                       | 0                      | 0                      | 0            |
| 1     | 20               | 0               | 0            | 0             | 0           | 0              | 0                        | 0                        | 0                       | 0              | 0            | 0                       | 0                      | 0                      | 0            |
| 2     | 400              | 0               | 0            | 0             | 0           | 0              | 0                        | 0                        | 0                       | 0              | 0            | 0                       | 0                      | 0                      | 0            |
| 3     | 8902             | 34              | 0            | 0             | 0           | 12             | 0                        | 0                        | 0                       | 12             | 0            | 0                       | 0                      | 0                      | 0            |
| 4     | 197281           | 1576            | 0            | 0             | 0           | 461            | 0                        | 0                        | 0                       | 461            | 8            | 0                       | 0                      | 0                      | 8            |
| 5     | 4865609          | 82719           | 258          | 0             | 0           | 26998          | 6                        | 0                        | 0                       | 27004          | 347          | 0                       | 0                      | 0                      | 347          |
| 6     | 119060324        | 2812008         | 5248         | 0             | 0           | 797896         | 329                      | 46                       | 0                       | 798271         | 10828        | 0                       | 0                      | 0                      | 10828        |
| 7     | 3195901860       | 108329926       | 319617       | 883453        | 0           | 32648427       | 18026                    | 1628                     | 0                       | 32668081       | 435767       | 0                       | 0                      | 0                      | 435767       |
| 8     | 84998978956      | 3523740106      | 7187977      | 23605205      | 0           | 958135303      | 847039                   | 147215                   | 0                       | 959129557      | 9852032      | 4                       | 0                      | 0                      | 9852036      |
| 9     | 2439530234167    | 125208536153    | 319496827    | 1784356000    | 17334376    | 35653060996    | 37101713                 | 5547221                  | 10                      | 35695709940    | 399421379    | 1869                    | 768715                 | 0                      | 400191963    |
| 10    | 69352859712417   | 4092784875884   | 7824835694   | 50908510199   | 511374376   | 1077020493859  | 1531274015               | 302900733                | 879                     | 1078854669486  | 8771693969   | 598058                  | 18327128               | 0                      | 8790619155   |
| 11    | 2097651003696806 | 142537161824567 | 313603617408 | 2641343463566 | 49560932860 | 39068470901662 | 67494850305              | 11721852393              | 57443                   | 39147687661803 | 360675926605 | 60344676                | 1553739626             | 0                      | 362290010907 |
```
## Get involved
The project is still in it's infancy but I'm hoping to attract others to get involved, either through collaboration by working with me on the default move generator or through competition by writing your own move generator that out performs this one! 

## Find out more
[Read the docs](https://timmoth.github.io/grandchesstree/)
