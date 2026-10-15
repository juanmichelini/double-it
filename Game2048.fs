namespace Game2048

open System

// Expose Direction as a proper enum for C# interop
type Direction =
    | Up = 0
    | Down = 1
    | Left = 2
    | Right = 3

// Define Board as a class for better C# interop
type Board(data: int[,]) =
    member val Data = data with get, set

    new() = Board(Array2D.create 4 4 0)

    static member Empty() =
        new Board(Array2D.create 4 4 0)

    member this.Item
        with get(i, j) = this.Data.[i, j]
        and set(i, j) v = this.Data.[i, j] <- v

// Game logic module
module Game =
    let WINNING_NUMBER = 2048
    let BOARD_SIZE = 4

    let toArray (board: Board) = board.Data
    let fromArray (arr: int[,]) = new Board(arr)

    let emptyBoard() : Board = Board.Empty()

    let removeEmptyCells (row: int[]) =
        row |> Array.filter (fun cell -> cell <> 0)

    let rec sumEquals (cells: int[]) =
        match cells with
        | [||] -> [||]
        | [|x|] -> [|x|]
        | [|x; y|] when x = y -> [|x + y|]
        | [|x; y|] -> [|x; y|]
        | _ ->
            if cells.[0] = cells.[1] then
                Array.append [|cells.[0] + cells.[1]|] (sumEquals cells.[2..])
            else
                Array.append [|cells.[0]|] (sumEquals cells.[1..])

    let addMissingCells (cells: int[]) : int[] =
        let paddingSize = BOARD_SIZE - cells.Length
        Array.append cells (Array.create paddingSize 0)

    let moveLeftRow (row: int[]) : int[] =
        let nonEmptyCells = removeEmptyCells row
        let summedCells = sumEquals nonEmptyCells
        addMissingCells summedCells

    let getRow (board: Board) (rowIndex: int) : int[] =
        [| for j in 0..BOARD_SIZE-1 -> board.[rowIndex, j] |]

    let setRow (board: Board) (rowIndex: int) (row: int[]) : Board =
        let newBoard = new Board(Array2D.copy board.Data)
        for j in 0..BOARD_SIZE-1 do
            newBoard.[rowIndex, j] <- row.[j]
        newBoard

    let getColumn (board: Board) (colIndex: int) : int[] =
        [| for i in 0..BOARD_SIZE-1 -> board.[i, colIndex] |]

    let setColumn (board: Board) (colIndex: int) (col: int[]) : Board =
        let newBoard = new Board(Array2D.copy board.Data)
        for i in 0..BOARD_SIZE-1 do
            newBoard.[i, colIndex] <- col.[i]
        newBoard

    let moveLeft (board: Board) : Board =
        let mutable newBoard = new Board(Array2D.copy board.Data)
        for i in 0..BOARD_SIZE-1 do
            let row = getRow newBoard i
            let newRow = moveLeftRow row
            newBoard <- setRow newBoard i newRow
        newBoard

    let rotateClockwise (board: Board) : Board =
        let newBoard = new Board(Array2D.create BOARD_SIZE BOARD_SIZE 0)
        for i in 0..BOARD_SIZE-1 do
            for j in 0..BOARD_SIZE-1 do
                newBoard.[j, BOARD_SIZE-1-i] <- board.[i, j]
        newBoard

    let rotateCounterClockwise (board: Board) : Board =
        let newBoard = new Board(Array2D.create BOARD_SIZE BOARD_SIZE 0)
        for i in 0..BOARD_SIZE-1 do
            for j in 0..BOARD_SIZE-1 do
                newBoard.[BOARD_SIZE-1-j, i] <- board.[i, j]
        newBoard

    let rotate180 (board: Board) : Board =
        let newBoard = new Board(Array2D.create BOARD_SIZE BOARD_SIZE 0)
        for i in 0..BOARD_SIZE-1 do
            for j in 0..BOARD_SIZE-1 do
                newBoard.[BOARD_SIZE-1-i, BOARD_SIZE-1-j] <- board.[i, j]
        newBoard

    let move (dir: Direction) (board: Board) : Board =
        match dir with
        | Direction.Left -> moveLeft board
        | Direction.Right ->
            board
            |> rotate180
            |> moveLeft
            |> rotate180
        | Direction.Up ->
            board
            |> rotateClockwise
            |> moveLeft
            |> rotateCounterClockwise
        | Direction.Down ->
            board
            |> rotateCounterClockwise
            |> moveLeft
            |> rotateClockwise
        | _ -> board

    let findEmptyCells (board: Board) : (int * int) list =
        let mutable emptyCells = []
        for i in 0..BOARD_SIZE-1 do
            for j in 0..BOARD_SIZE-1 do
                if board.[i, j] = 0 then
                    emptyCells <- (i, j) :: emptyCells
        emptyCells

    let putTileAt (board: Board) (row, col) (value: int) : Board =
        let newBoard = new Board(Array2D.copy board.Data)
        newBoard.[row, col] <- value
        newBoard

    let random = Random()

    let sproutTile (board: Board) : Board =
        let emptyCells = findEmptyCells board
        if List.isEmpty emptyCells then
            board
        else
            let index = random.Next(emptyCells.Length)
            let (row, col) = emptyCells.[index]
            let value = if random.NextDouble() > 0.5 then 2 else 1
            putTileAt board (row, col) value

    let sproutTiles (board: Board) : Board =
        if random.NextDouble() > 0.5 then
            board |> sproutTile |> sproutTile
        else
            board |> sproutTile

    let boardsEqual (b1: Board) (b2: Board) : bool =
        let mutable equal = true
        for i in 0..BOARD_SIZE-1 do
            for j in 0..BOARD_SIZE-1 do
                if b1.[i, j] <> b2.[i, j] then
                    equal <- false
        equal

    let hasWon (board: Board) : bool =
        let mutable found = false
        for i in 0..BOARD_SIZE-1 do
            for j in 0..BOARD_SIZE-1 do
                if board.[i, j] >= WINNING_NUMBER then
                    found <- true
        found

    let hasValidMove (board: Board) : bool =
        let directions = [Direction.Up; Direction.Down; Direction.Left; Direction.Right]
        directions |> List.exists (fun dir ->
            let newBoard = move dir board
            not (boardsEqual board newBoard))
