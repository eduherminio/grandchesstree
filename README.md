# The Grand Chess Tree
The grand chess tree is a public distributed effort to traverse the depths of the game of chess.
```
 | depth | nodes         | captures     | enpassants | castles    | promotions | checks      | discovered_checks | double_checks | check_mates |
 |-------|---------------|--------------|------------|------------|------------|-------------|-------------------|---------------|-------------|
 | 0     | 1             | 0            | 0          | 0          | 0          | 0           | 0                 | 0             | 0           |
 | 1     | 20            | 0            | 0          | 0          | 0          | 0           | 0                 | 0             | 0           |
 | 2     | 400           | 0            | 0          | 0          | 0          | 0           | 0                 | 0             | 0           |
 | 3     | 8902          | 34           | 0          | 0          | 0          | 12          | 0                 | 0             | 8           |
 | 4     | 197281        | 1576         | 0          | 0          | 0          | 469         | 0                 | 0             | 8           |
 | 5     | 4865609       | 82719        | 258        | 0          | 0          | 27351       | 6                 | 0             | 347         |
 | 6     | 119060324     | 2812008      | 5248       | 0          | 0          | 809099      | 329               | 46            | 10828       |
 | 7     | 3195901860    | 108329926    | 319617     | 883453     | 0          | 33103848    | 18026             | 1628          | 435765      |
 | 8     | 84998978956   | 3523740106   | 7187977    | 23605205   | 0          | 968981593   | 847043            | 147215        | 9851894     |
 | 9     | 2439530234167 | 125208536153 | 319496827  | 1784356000 | 17334376   | 36095901903 | 37103592          | 6315946       | 400182381   |
 ```
## Client
The client project pulls work from the api and then distributes it to a number of workers running on the local machine. I'm currently generating ~6bnps accross all 32 threads, whilst this may sound like a lot, it's peanuts compared to the insane size of the chess tree. A GPU client worker will be necessary to reach new depths.
Whilst running the following table will be updated in the console reporting on the progress of the current task.
```
-------------------------------
 | key               | value   |
 -------------------------------
 | depth             | 9       |
 -------------------------------
 | workers           | 32      |
 -------------------------------
 | processed         | 396/400 |
 -------------------------------
 | single_nps        | 189.3m  |
 -------------------------------
 | combined_nps      | 6.1b    |
 -------------------------------
 | nodes             | 2.4t    |
 -------------------------------
 | captures          | 122.9b  |
 -------------------------------
 | enpassants        | 313.3m  |
 -------------------------------
 | castles           | 1.7b    |
 -------------------------------
 | promotions        | 17.1m   |
 -------------------------------
 | checks            | 35.4b   |
 -------------------------------
 | discovered_checks | 37m     |
 -------------------------------
 | double_checks     | 6.3m    |
 -------------------------------
 | check_mates       | 394.3m  |
 -------------------------------
```
Once the task is completed the final results will be output to the console:
```
All workers have completed.
---------------
nps:6.1b
nodes:2439530234167
captures:125208536153
enpassants:319496827
castles:1784356000
promotions:17334376
checks:36095901903
discovered_checks:37103592
double_checks:6315946
check_mates:400182381
---------------
```

## Client Worker
The client worker contains a move generator and responds to a simple protocol. On my machine using a single thread and a 1GB hash table it's calculating around 300mnps. Outputting the number of `nodes`, `captures`, `enpassants`, `castles`, `promotions`, `checks`, `discovered_checks`, `double_checks` and `check_mates` found for any given fen / depth.

### Protocol
- <-`ready` - the worker sends 'ready' when it's ready to recieve a new perft command
- ->`begin:<depth>:<fen>` - sending a ready worker the begin command will start calculating the perft results to the given depth for the fen provided
- <-`processing` - the worker will respond to the begin command with 'processing' to indicate that it has accepted the command and started processing it
Once complete the worker will output the following results:
- <-`fen:<string>`
- <-`hash:<ulong>`
- <-`nps:<ulong>`
- <-`nodes:<ulong>`
- <-`captures:<ulong>`
- <-`enpassants:<ulong>`
- <-`castles:<ulong>`
- <-`promotions:<ulong>`
- <-`checks:<ulong>`
- <-`discovered_checks:<ulong>`
- <-`double_checks:<ulong>`
- <-`check_mates:<ulong>`
- <-`done` - indicates that the worker has finished and ready to recieve a new command
- ->`reset` - This will clear the hash table and reset all internal state
- <-`ready` - the reset command will output ready when it's finished
- ->`quit` - the quit command will gracefully terminate the process

Here's an example of what the console output looks like:
```
ready
begin:7:rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
processing
fen:rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -
hash:5060803636482931868
nps:291277980
nodes:3195901860
captures:108329926
enpassants:319617
castles:883453
promotions:0
checks:33103848
discovered_checks:18026
double_checks:1628
check_mates:435765
done
```

## Api
Todo - this project will contain the web app and api that clients request work from and submit results to.
