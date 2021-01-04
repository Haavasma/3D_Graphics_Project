package tcpreader

import (
	"encoding/json"
	"fmt"
	"net"
	"os"
	"time"

	"github.com/google/uuid"

	"../data"
)

var queue = data.SafeQueue{Q: []net.Conn{}}
var channels = data.SafeChannelTCPConnSlice{V: make(map[string][]net.Conn)}

// ReadTCP reads tcp
func ReadTCP(socket net.Listener) {
	fmt.Println("Reading tcp")
	for {
		conn, err := socket.Accept()
		fmt.Println("connection requested", conn)
		if err != nil {
			os.Exit(1)
		}
		go handleConnection(conn)
	}
}

// reads messages from given connection
func handleConnection(conn net.Conn) {
	for {
		var buf [1024]byte
		n, err := conn.Read(buf[0:])
		if err != nil {
			fmt.Println("Error Reading")
			conn.Close()
			handleDisconnect(conn)
			break
		}
		go handleMessage(conn, buf, n)
	}
}

// handles a message from a connection
func handleMessage(conn net.Conn, buf [1024]byte, n int) {
	var result map[string]interface{}

	json.Unmarshal(buf[0:n], &result)

	switch res := result["type"]; res {
	case "queue":
		handleQueue(conn, result)
	case "dequeue":
		handleDeQueue(conn)
	case "endTurn":
		handleEndTurn(conn, result)
	case "gameLost":
		handleGameLost(conn, result)
	}
}

// CheckQueue checks the queue
func CheckQueue() {
	for {
		fmt.Println("checking queue")
		queue.Mux.Lock()
		// sends ping to everyone in queue to keep connection active
		for i := 0; i < len(queue.Q); i++ {
			pingObj := make(map[string]interface{})
			pingObj["type"] = "ping"

			bytes, err := json.Marshal(pingObj)
			if err != nil {
				fmt.Println("Can't serialize", pingObj)
			}
			queue.Q[i].Write(bytes)
		}
		// check the queue and pair up adjacent queued connections
		if len(queue.Q) > 1 {
			channels.Mux.Lock()
			for {
				// Create new channel id
				newChannelUUID := uuid.New().String()

				// create message
				sendObj := make(map[string]interface{})
				sendObj["channel"] = newChannelUUID
				sendObj["type"] = "newGame"
				sendObj["myTurn"] = false

				// Send myturn true to one and false to other
				bytes1, err := json.Marshal(sendObj)
				if err != nil {
					fmt.Println("Can't serialize", sendObj)
				}

				sendObj["myTurn"] = true

				bytes2, err := json.Marshal(sendObj)
				if err != nil {
					fmt.Println("Can't serialize", sendObj)
				}
				queue.Q[0].Write(bytes1)
				queue.Q[1].Write(bytes2)

				channels.V[newChannelUUID] = append(channels.V[newChannelUUID], queue.Q[0], queue.Q[1])

				queue.Q = queue.Q[2:]
				if len(queue.Q) < 2 {
					break
				}
			}
			channels.Mux.Unlock()
		}
		fmt.Println(queue.Q)
		queue.Mux.Unlock()
		// repeat check every 3 seconds
		time.Sleep(3 * time.Second)
	}
}

// adds the tcp connection to the game queue
func handleQueue(conn net.Conn, result map[string]interface{}) {
	queue.Mux.Lock()
	queue.Q = append(queue.Q, conn)
	queue.Mux.Unlock()
}

// removes the connection from the game queue
func handleDeQueue(conn net.Conn) {
	fmt.Println("got dequeue request")

	queue.Mux.Lock()

	for i := 0; i < len(queue.Q); i++ {
		if queue.Q[i] == conn {
			queue.Q = append(queue.Q[:i], queue.Q[i+1:]...)
			break
		}
	}

	queue.Mux.Unlock()
}

// handles end turn message
func handleEndTurn(conn net.Conn, result map[string]interface{}) {
	channel, ok := result["channel"].(string)

	fmt.Println("got turn end request")
	if !ok {
		fmt.Println("could not read channel")
		return
	}
	// get correct connections to send message to
	connToSend := channels.V[channel]

	// send toggleturn message to everyone in channel
	sendObj := make(map[string]interface{})
	sendObj["type"] = "toggleTurn"
	bytes, err := json.Marshal(sendObj)
	if err != nil {
		fmt.Println("Can't serialize", sendObj)
	}

	for _, conn := range connToSend {
		conn.Write(bytes)
	}
}

// handles game lost message from client
func handleGameLost(conn net.Conn, result map[string]interface{}) {
	channel, ok := result["channel"].(string)

	fmt.Println("Handling lost game")
	if !ok {
		fmt.Println("could not read channel")
		return
	}

	// find connections to send message to
	connToSend := channels.V[channel]

	// Send result 0 to loser and 1 to others
	sendObj := make(map[string]interface{})
	sendObj["type"] = "EndGame"
	sendObj["result"] = 1
	bytes, err := json.Marshal(sendObj)
	if err != nil {
		fmt.Println("Can't serialize", sendObj)
	}
	sendObj["result"] = 0
	bytes2, err := json.Marshal(sendObj)

	if err != nil {
		fmt.Println("Can't serialize", sendObj)
	}

	for _, c := range connToSend {
		if conn != c {
			fmt.Println("Sending to other that they won")
			c.Write(bytes)
		} else {
			fmt.Println("Sending to sender that they lost")
			c.Write(bytes2)
		}
	}
	// remove from channels
	channels.Mux.Lock()
	delete(channels.V, channel)
	channels.Mux.Unlock()
}

// handles disconnect
func handleDisconnect(conn net.Conn) {
	fmt.Println("handling disconnect")
	// reports loss to correct connection by finding the disconnected connection in channels
	for channel, x := range channels.V {
		for _, c := range x {
			if c == conn {
				fmt.Println("found connection to report loss")
				result := make(map[string]interface{})
				result["channel"] = channel
				go handleGameLost(conn, result)
				break
			}
		}
	}
	handleDeQueue(conn)
}
