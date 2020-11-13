package tcpserver

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
	go checkQueue()
	for {
		conn, err := socket.Accept()
		fmt.Println("connection requested", conn)
		if err != nil {
			os.Exit(1)
		}
		go handleConnection(conn)
	}
}

func handleConnection(conn net.Conn) {
	for {
		var buf [1024]byte
		n, err := conn.Read(buf[0:])
		if err != nil {
			fmt.Println("Error Reading")
			conn.Close()
			break
		}
		go handleMessage(conn, buf, n)
	}
}

func handleMessage(conn net.Conn, buf [1024]byte, n int) {
	var result map[string]interface{}

	json.Unmarshal(buf[0:n], &result)

	switch res := result["type"]; res {
	case "queue":
		handleQueue(conn, result)
	case "endTurn":
		handleEndTurn(conn, result)
	}
}

func checkQueue() {

	for {
		queue.Mux.Lock()
		for i := 0; i < len(queue.Q); i++ {
			pingObj := make(map[string]interface{})
			pingObj["type"] = "ping"

			bytes, err := json.Marshal(pingObj)
			if err != nil {
				fmt.Println("Can't serialize", pingObj)
			}
			queue.Q[i].Write(bytes)
		}
		if len(queue.Q) > 1 {
			for {
				newChannelUUID := uuid.New().String()

				sendObj := make(map[string]interface{})
				sendObj["channel"] = newChannelUUID
				sendObj["type"] = "newGame"
				sendObj["myTurn"] = false

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

				channels.Mux.Lock()
				channels.V[newChannelUUID] = append(channels.V[newChannelUUID], queue.Q[0], queue.Q[1])
				channels.Mux.Unlock()

				queue.Q = queue.Q[2:]

				if len(queue.Q) < 2 {
					break
				}
			}
		}
		queue.Mux.Unlock()
		time.Sleep(2 * time.Second)
	}
}

func handleQueue(conn net.Conn, result map[string]interface{}) {
	queue.Mux.Lock()
	queue.Q = append(queue.Q, conn)
	queue.Mux.Unlock()
}

func handleEndTurn(conn net.Conn, result map[string]interface{}) {
	channel, ok := result["channel"].(string)

	if !ok {
		fmt.Println("could not read channel")
		return
	}
	connToSend := channels.V[channel]

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
