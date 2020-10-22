package main

import (
	"fmt"
	"net"
	"os"

	"../tcpserver"
	"../udpserver"
)

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

	tcpSocket, err := net.Listen("tcp", "127.0.0.1:8081")
	if err != nil {
		os.Exit(1)
	}
	defer tcpSocket.Close()

	fmt.Println("\nReading " + protocol + " from " + udpAddr.String())

	udpConn, err := net.ListenUDP(protocol, udpAddr)
	if err != nil {
		fmt.Println(err)
	}
	defer udpConn.Close()

	go udpserver.ReadUDP(udpConn)

	tcpserver.ReadTCP(tcpSocket)
}

/*
func readUDP(conn *net.UDPConn) {
	for {
		var buf [1024]byte
		n, addr, err := conn.ReadFromUDP(buf[0:])
		if err != nil {
			fmt.Println("Error Reading")
			return
		}
		go handleMessage(conn, buf, n, addr)
	}
}

func handleMessage(conn *net.UDPConn, buf [1024]byte, n int, addr *net.UDPAddr) {
	var result map[string]interface{}

	json.Unmarshal(buf[0:n], &result)

	switch res := result["type"]; res {
	case "transform":
		handleTransform(conn, result, addr)
	case "queue":
		handleQueue(conn, result, addr)
	case "toggleTurn":
		handleToggleTurn(conn, result, addr)
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

func handleQueue(conn *net.UDPConn, result map[string]interface{}, addr *net.UDPAddr) {
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

			conn.WriteTo(bytes1, queue.q[0])
			conn.WriteTo(bytes2, queue.q[1])

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

func handleToggleTurn(conn *net.UDPConn, result map[string]interface{}, addr *net.UDPAddr) {

}
*/
