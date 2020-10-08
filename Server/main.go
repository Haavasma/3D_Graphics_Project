package main

import (
	"encoding/json"
	"fmt"
	"net"
	"sync"
)

type SafeChannelAddressSlice struct {
	v   [][]*net.UDPAddr
	mux sync.Mutex
}

type SafeQueue struct {
	q   []*net.UDPAddr
	mux sync.Mutex
}

var channels = SafeChannelAddressSlice{v: [][]*net.UDPAddr{}}
var queue = SafeQueue{q: []*net.UDPAddr{}}

//yo
func main() {
	//Basic variables
	port := ":8080"
	protocol := "udp"

	//Build the address
	udpAddr, err := net.ResolveUDPAddr(protocol, port)
	if err != nil {
		fmt.Println("Wrong Address")
		return
	}

	//Output
	fmt.Println("\nReading " + protocol + " from " + udpAddr.String())

	//Create the connection
	udpConn, err := net.ListenUDP(protocol, udpAddr)
	if err != nil {
		fmt.Println(err)
	}
	defer udpConn.Close()
	//Keep calling this function
	display(udpConn)
}

func display(conn *net.UDPConn) {
	for {
		var buf [1024]byte
		n, addr, err := conn.ReadFromUDP(buf[0:])
		fmt.Println(n)
		if err != nil {
			fmt.Println("Error Reading")
			return
		} else {
			go handleMessage(conn, buf, n, addr)
		}
	}
}

func handleMessage(conn *net.UDPConn, buf [1024]byte, n int, addr *net.UDPAddr) {
	var result map[string]interface{}

	json.Unmarshal(buf[0:n], &result)

	switch res := result["type"]; res {
	case "transform":
		fmt.Println("handle tranform")
		handleTransform(conn, result, addr)
	case "queue":
		fmt.Println("handle queue")
		handleQueue(conn, result, n, addr)
	}
}

func handleTransform(conn *net.UDPConn, result map[string]interface{}, addr *net.UDPAddr) {
	fmt.Println(result["channel"])
	channel := int(result["channel"].(float64))
	addressesToSend := channels.v[channel]

	bytes, err := json.Marshal(result)
	if err != nil {
		fmt.Println("Can't serislize", result)
	}
	for _, address := range addressesToSend {
		if address != addr {
			conn.WriteTo(bytes, address)
		}
	}
}

func handleQueue(conn *net.UDPConn, result map[string]interface{}, n int, addr *net.UDPAddr) {
	queue.mux.Lock()
	queue.q = append(queue.q, addr)

	channels.mux.Lock()

	if len(queue.q) > 1 {
		for {
			newChannelindex := len(channels.v)
			channels.v = append(channels.v, []*net.UDPAddr{queue.q[0], queue.q[1]})

			sendObj := make(map[string]interface{})

			sendObj["channel"] = newChannelindex
			sendObj["type"] = "newGame"

			bytes, err := json.Marshal(sendObj)
			if err != nil {
				fmt.Println("Can't serislize", sendObj)
			}

			conn.WriteTo(bytes, queue.q[0])
			conn.WriteTo(bytes, queue.q[1])

			queue.q = queue.q[2:]

			if len(queue.q) < 2 {
				break
			}
		}
	}
	fmt.Println(channels.v)
	fmt.Println(queue.q)
	channels.mux.Unlock()

	queue.mux.Unlock()
}
